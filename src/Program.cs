using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace PrivateGalleryCreator
{
  internal class Program
  {
    private const string _xmlFileName = "feed.xml";
    private static string _dir;
    private static string _galleryName;
    private static bool _recursive = false;
    private static bool _latestOnly = false;
    private static string _outputFile;
    private static string _exclude = string.Empty;
    private static string _devVersion;

    /// <summary>
    /// When not empty, this folder path will be used as download source for the extensions.
    /// </summary>
    private static string _source;

    private static void Main(string[] args)
    {
      _dir = args.FirstOrDefault(a => a.StartsWith("--input="))?.Replace("--input=", string.Empty) ?? Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

      _recursive = args.Any(a => a == "--recursive");

      _latestOnly = args.Any(a => a == "--latest-only");

      _outputFile = args.FirstOrDefault(a => a.StartsWith("--output="))?.Replace("--output=", string.Empty) ?? Path.Combine(_dir, _xmlFileName);

      _exclude = args.FirstOrDefault(a => a.StartsWith("--exclude="))?.Replace("--exclude=", string.Empty) ?? string.Empty;

      _galleryName = args.FirstOrDefault(a => a.StartsWith("--name="))?.Replace("--name=", string.Empty) ?? "VSIX Gallery";

      _source = args.FirstOrDefault(a => a.StartsWith("--source="))?.Replace("--source=", string.Empty) ?? string.Empty;

      _devVersion = args.FirstOrDefault(a => a.StartsWith("--version="))?.Replace("--version=", string.Empty) ?? "17.0";

      if (Convert.ToSingle(_devVersion.Split('.')[0]) >= 18.0 || Convert.ToSingle(_devVersion.Split('.')[0]) < 11.0 )
      {
        Console.WriteLine("The version number is incorrect, please enter the version number of Visual Studio");
      }
      else
      {
        GenerateAtomFeed();
      }

      switch (args)
      {
        case var a when (a.Contains("--watch") || a.Contains("-w")):
          WatchDirectoryForChanges();
          break;

        case var a when (a.Contains("--terminate") || a.Contains("-t")):
          break;

        default:
          Console.WriteLine("Press any key to close...");
          Console.ReadKey(true);
          break;
      }
    }

    private static void WatchDirectoryForChanges()
    {
      var fsw = new FileSystemWatcher(_dir, "*.vsix");
      fsw.Changed += FileChanged;
      fsw.Created += FileChanged;
      fsw.Deleted += FileChanged;
      fsw.Renamed += FileChanged;
      fsw.IncludeSubdirectories = false;
      fsw.EnableRaisingEvents = true;

      Console.WriteLine("Watching for file changes...");

      while (true)
      {
        System.Threading.Thread.Sleep(1000);
      }
    }

    private static void FileChanged(object sender, EventArgs e)
    {
      try
      {
        GenerateAtomFeed();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.Write(ex);
      }
    }

    private static void GenerateAtomFeed()
    {
      var packageFiles = EnumerateFilesSafe(new DirectoryInfo(_dir), "*.vsix", _recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Distinct();
      var filteredPackageFiles = string.IsNullOrEmpty(_exclude) ? packageFiles : packageFiles.Where(f => !f.FullName.Contains(_exclude));
      var packagesToProcess = filteredPackageFiles.Select(f => ProcessVsix(f.FullName));
      if (_latestOnly)
      {
        packagesToProcess = packagesToProcess.GroupBy(p => p.ID).Select(g => g.OrderByDescending(pkg => Version.Parse(pkg.Version)).First());
      }

      var writer = new FeedWriter(_galleryName);
      string feedUrl = _outputFile;
      string xml = writer.GetFeed(feedUrl, packagesToProcess);

      File.WriteAllText(feedUrl, xml, Encoding.UTF8);

      Console.WriteLine();
      Console.WriteLine($"{_outputFile} generated successfully");
    }

    private static Package ProcessVsix(string sourceVsixPath)
    {
      string temp = Path.GetTempPath();
      string tempFolder = Path.Combine(temp, Guid.NewGuid().ToString());

      try
      {
        Directory.CreateDirectory(tempFolder);
        ZipFile.ExtractToDirectory(sourceVsixPath, tempFolder);

        var vsixFile = Path.GetFileName(sourceVsixPath);
        string vsixSourcePath = null;

        if(String.IsNullOrEmpty(_source))
        {
            vsixSourcePath = sourceVsixPath;
        }
        else
        {
            string subPath = Path.GetRelativePath(_dir, sourceVsixPath);
            if (Uri.IsWellFormedUriString(_source, UriKind.Absolute))
            {
                UriBuilder uriBuilder = new UriBuilder(_source);
                uriBuilder.Path += subPath;
                vsixSourcePath = uriBuilder.Uri.ToString();
            }
            else
            {
                vsixSourcePath = Path.Combine(_source, subPath);
            }
        }

        var parser = new VsixManifestParser();
        Package package = parser.CreateFromManifest(tempFolder, vsixFile, vsixSourcePath);

        package.DevVersion = _devVersion;

        if (!string.IsNullOrEmpty(package.Icon))
        {
          string currentDir = Path.GetDirectoryName(_outputFile);
          string sourceIconPath = Path.Combine(tempFolder, package.Icon);

          if (File.Exists(sourceIconPath))
          {
            string iconDir = Path.Combine(currentDir, "icons");
            string icon = Path.Combine(iconDir, package.ID + Path.GetExtension(package.Icon));

            if (!Directory.Exists(iconDir))
            {
              DirectoryInfo dir = Directory.CreateDirectory(iconDir);
              dir.Attributes |= FileAttributes.Hidden;
            }

            File.Copy(sourceIconPath, icon, true);
          }
        }

        Console.WriteLine($"Parsed {package.FileName}");

        return package;
      }
      finally
      {
        Directory.Delete(tempFolder, true);
      }
    }

    public static IEnumerable<FileInfo> EnumerateFilesSafe(DirectoryInfo dir, string filter = "*.*", SearchOption opt = SearchOption.TopDirectoryOnly)
    {
      var retval = Enumerable.Empty<FileInfo>();

      try
      {
        retval = dir.EnumerateFiles(filter, SearchOption.TopDirectoryOnly);
      }
      catch
      {
        Console.WriteLine("{0} Inaccessable.", dir.FullName);
      }

      if (opt == SearchOption.AllDirectories)
      {
        retval = retval.Concat(EnumerateDirectoriesSafe(dir, opt: opt).SelectMany(x => EnumerateFilesSafe(x, filter, SearchOption.TopDirectoryOnly)));
      }

      return retval;
    }

    public static IEnumerable<DirectoryInfo> EnumerateDirectoriesSafe(DirectoryInfo dir, string filter = "*.*", SearchOption opt = SearchOption.TopDirectoryOnly)
    {
      var retval = Enumerable.Empty<DirectoryInfo>();

      try
      {
        retval = dir.EnumerateDirectories(filter, SearchOption.TopDirectoryOnly);
      }
      catch
      {
        Console.WriteLine("{0} Inaccessable.", dir.FullName);
      }

      if (opt == SearchOption.AllDirectories)
      {
        retval = retval.Concat(retval.SelectMany(x => EnumerateDirectoriesSafe(x, filter, opt)));
      }

      return retval;
    }
  }
}
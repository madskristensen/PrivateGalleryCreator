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

        private static void Main(string[] args)
        {
            _dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            GenerateAtomFeed();

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
            IEnumerable<Package> packages = Directory.EnumerateFiles(_dir, "*.vsix", SearchOption.TopDirectoryOnly)
                                                .Select(f => ProcessVsix(f));

            var writer = new FeedWriter();
            string feedUrl = Path.Combine(_dir, _xmlFileName);
            string xml = writer.GetFeed(feedUrl, packages);

            File.WriteAllText(feedUrl, xml, Encoding.UTF8);

            Console.WriteLine();
            Console.WriteLine($"{_xmlFileName} generated successfully");
        }

        private static Package ProcessVsix(string sourceVsixPath)
        {
            string temp = Path.GetTempPath();
            string tempFolder = Path.Combine(temp, Guid.NewGuid().ToString());
            
            try
            {
                Directory.CreateDirectory(tempFolder);
                ZipFile.ExtractToDirectory(sourceVsixPath, tempFolder);

                var parser = new VsixManifestParser();
                Package package = parser.CreateFromManifest(tempFolder, sourceVsixPath);
                

                if (!string.IsNullOrEmpty(package.Icon))
                {
                    string currentDir = Path.GetDirectoryName(sourceVsixPath);
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
    }
}
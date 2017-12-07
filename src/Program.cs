using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace PrivateGalleryCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var packages = Directory.GetFiles(dir, "*.vsix", SearchOption.TopDirectoryOnly)
                                    .Select(f => ProcessVsix(f))
                                    .ToArray();

            var writer = new FeedWriter();
            var feedUrl = Path.Combine(dir, "feed.xml");
            string xml = writer.GetFeed(feedUrl, packages);

            File.WriteAllText(feedUrl, xml, Encoding.UTF8);

            Console.WriteLine();
            Console.WriteLine("Feed generated (" + packages.Count() + " extensions)");
        }

        private static Package ProcessVsix(string sourceVsixPath)
        {
            string temp = Path.GetTempPath();
            string tempFolder = Path.Combine(temp, Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempFolder);
                ZipFile.ExtractToDirectory(sourceVsixPath, tempFolder);

                VsixManifestParser parser = new VsixManifestParser();
                Package package = parser.CreateFromManifest(tempFolder, sourceVsixPath);

                if (!string.IsNullOrEmpty(package.Icon))
                {
                    string currentDir = Path.GetDirectoryName(sourceVsixPath);
                    string sourceIconPath = Path.Combine(tempFolder, package.Icon);
                    string iconDir = Path.Combine(currentDir, "icons");
                    string icon = Path.Combine(iconDir, package.ID + Path.GetExtension(package.Icon));

                    if (!Directory.Exists(iconDir))
                    {
                        var dir = Directory.CreateDirectory(iconDir);
                        dir.Attributes |= FileAttributes.Hidden;
                    }

                    File.Copy(sourceIconPath, icon, true);
                }

                Console.WriteLine("Parsed " + package.FileName);

                return package;
            }
            finally
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }
}

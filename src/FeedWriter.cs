using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace PrivateGalleryCreator
{
    public class FeedWriter
    {
    private readonly string galleryTitle;

    public FeedWriter(string galleryTitle)
    {
      this.galleryTitle = galleryTitle;
    }
        public string GetFeed(string fileName, IEnumerable<Package> packages)
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("feed", "http://www.w3.org/2005/Atom");

                writer.WriteElementString("title", galleryTitle);
                writer.WriteElementString("id", "5a7c2525-ddd8-4c44-b2e3-f57ba01a0d81");
                writer.WriteElementString("updated", DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ssZ"));
                writer.WriteElementString("subtitle", "Add this feed to Visual Studio's extension manager from Tools -> Options -> Environment -> Extensions and Updates");

                writer.WriteStartElement("link");
                writer.WriteAttributeString("rel", "alternate");
                writer.WriteAttributeString("href", Path.GetFileName(fileName));
                writer.WriteEndElement(); // link

                foreach (Package package in packages)
                {
                    AddEntry(writer, package, fileName);
                }

                writer.WriteEndElement(); // feed
            }

            return sb.ToString().Replace("utf-16", "utf-8");
        }

        private void AddEntry(XmlWriter writer, Package package, string baseUrl)
        {
            writer.WriteStartElement("entry");

            writer.WriteElementString("id", package.ID);

            writer.WriteStartElement("title");
            writer.WriteAttributeString("type", "text");
            writer.WriteValue(package.Name);
            writer.WriteEndElement(); // title

            writer.WriteStartElement("link");
            writer.WriteAttributeString("rel", "alternate");
            writer.WriteAttributeString("href", package.FileName);
            writer.WriteEndElement(); // link

            writer.WriteStartElement("summary");
            writer.WriteAttributeString("type", "text");
            writer.WriteValue(package.Description);
            writer.WriteEndElement(); // summary

            writer.WriteElementString("published", package.DatePublished.ToString("yyyy-MM-ddTHH\\:mm\\:ssZ"));
            writer.WriteElementString("updated", package.DatePublished.ToString("yyyy-MM-ddTHH\\:mm\\:ssZ"));

            writer.WriteStartElement("author");
            writer.WriteElementString("name", package.Author);
            writer.WriteEndElement(); // author

            writer.WriteStartElement("content");
            writer.WriteAttributeString("type", "application/octet-stream");
            writer.WriteAttributeString("src", package.FullPath);
            writer.WriteEndElement(); // content

            if (!string.IsNullOrEmpty(package.Icon))
            {
                writer.WriteStartElement("link");
                writer.WriteAttributeString("rel", "icon");
                writer.WriteAttributeString("href", "icons\\" + package.ID + Path.GetExtension(package.Icon));
                writer.WriteEndElement(); // icon
            }

            writer.WriteRaw("\r\n<Vsix xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://schemas.microsoft.com/developer/vsx-syndication-schema/2010\">\r\n");

            writer.WriteElementString("Id", package.ID);
            writer.WriteElementString("Version", package.Version);

            writer.WriteStartElement("References");
            writer.WriteEndElement();

            writer.WriteRaw("\r\n<Rating xsi:nil=\"true\" />");
            writer.WriteRaw("\r\n<RatingCount xsi:nil=\"true\" />");
            writer.WriteRaw("\r\n<DownloadCount xsi:nil=\"true\" />\r\n");

            if (package.ExtensionList?.Extensions != null)
            {
                if (package.DevVersion.Contains("17"))
                {
                    int ExtCount = package.ExtensionList.Extensions.Length;
                    for (int i = 0; i < ExtCount; i++)
                    {
                        writer.WriteElementString("PackedExtensionIDs", package.ExtensionList.Extensions.Select(e => e.VsixId).ElementAt(i));
                        writer.WriteString("\r\n");
                    }
                }
                else
                {
                    string ids = string.Join(";", package.ExtensionList.Extensions.Select(e => e.VsixId));
                    writer.WriteElementString("PackedExtensionIDs", ids);
                }
            }

            writer.WriteRaw("</Vsix>");// Vsix
            writer.WriteEndElement(); // entry
        }
    }
}

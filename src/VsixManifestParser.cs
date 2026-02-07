using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace PrivateGalleryCreator
{
    public partial class VsixManifestParser
    {
        public static Package CreateFromManifest(string tempFolder, string vsixFileName, string vsixSource)
        {
            string xml = File.ReadAllText(Path.Combine(tempFolder, "extension.vsixmanifest"));
            xml = XmlNamespaceRegex().Replace(xml, string.Empty);

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var package = new Package(vsixFileName, vsixSource);

            if (doc.GetElementsByTagName("DisplayName").Count > 0)
            {
                Vs2012Format(doc, package);
            }
            else
            {
                Vs2010Format(doc, package);
            }

            string license = ParseNode(doc, "License", false);
            if (!string.IsNullOrEmpty(license))
            {
                string path = Path.Combine(tempFolder, license);
                if (File.Exists(path))
                {
                    package.License = File.ReadAllText(path);
                }
            }

            AddExtensionList(package, tempFolder);

            return package;
        }

        private static void AddExtensionList(Package package, string tempFolder)
        {
            string vsext = Directory.EnumerateFiles(tempFolder, "*.vsext", SearchOption.AllDirectories).FirstOrDefault();

            if (!string.IsNullOrEmpty(vsext))
            {
                string json = File.ReadAllText(vsext);

                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(ExtensionList));
                    var list = (ExtensionList)serializer.ReadObject(ms);
                    package.ExtensionList = list;
                }
            }
        }

        private static void Vs2012Format(XmlDocument doc, Package package)
        {
            package.ID = ParseNode(doc, "Identity", true, "Id");
            package.Name = ParseNode(doc, "DisplayName", true);
            package.Description = ParseNode(doc, "Description", true);
            package.Version = new Version(ParseNode(doc, "Identity", true, "Version")).ToString();
            package.Author = ParseNode(doc, "Identity", true, "Publisher");
            package.Icon = ParseNode(doc, "Icon", false);
            package.Preview = ParseNode(doc, "PreviewImage", false);
            package.Tags = ParseNode(doc, "Tags", false);
            package.DatePublished = DateTime.UtcNow;
            package.InstallationTargets = GetInstallationTargets(doc);
            package.ReleaseNotesUrl = ParseNode(doc, "ReleaseNotes", false);
            package.GettingStartedUrl = ParseNode(doc, "GettingStartedGuide", false);
            package.MoreInfoUrl = ParseNode(doc, "MoreInfo", false);
        }

        private static void Vs2010Format(XmlDocument doc, Package package)
        {
            package.ID = ParseNode(doc, "Identifier", true, "Id");
            package.Name = ParseNode(doc, "Name", true);
            package.Description = ParseNode(doc, "Description", true);
            package.Version = new Version(ParseNode(doc, "Version", true)).ToString();
            package.Author = ParseNode(doc, "Author", true);
            package.Icon = ParseNode(doc, "Icon", false);
            package.Preview = ParseNode(doc, "PreviewImage", false);
            package.DatePublished = DateTime.UtcNow;
            package.InstallationTargets = GetInstallationTargets(doc);
            package.ReleaseNotesUrl = ParseNode(doc, "ReleaseNotes", false);
            package.GettingStartedUrl = ParseNode(doc, "GettingStartedGuide", false);
            package.MoreInfoUrl = ParseNode(doc, "MoreInfo", false);
        }

        private static List<InstallationTarget> GetInstallationTargets(XmlDocument doc)
        {
            XmlNodeList list = doc.GetElementsByTagName("InstallationTarget");

            if (list.Count == 0)
                list = doc.GetElementsByTagName("VisualStudio");

            var targets = new List<InstallationTarget>();

            foreach (XmlNode node in list)
            {
                string identifier = node.Attributes?["Id"]?.Value;
                string versionRange = node.Attributes?["Version"]?.Value;

                if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(versionRange))
                    continue;

                string architecture = node["ProductArchitecture"]?.InnerText;
                targets.Add(new InstallationTarget(identifier, versionRange, architecture));
            }

            return targets;
        }

        private static string ParseNode(XmlDocument doc, string name, bool required, string attribute = "")
        {
            XmlNodeList list = doc.GetElementsByTagName(name);

            if (list.Count > 0)
            {
                XmlNode node = list[0];

                if (string.IsNullOrEmpty(attribute))
                    return node.InnerText;

                XmlAttribute attr = node.Attributes[attribute];

                if (attr != null)
                    return attr.Value;
            }

            if (required)
            {
                string message = string.Format("Attribute '{0}' could not be found on the '{1}' element in the .vsixmanifest file.", attribute, name);
                throw new Exception(message);
            }

            return null;
        }

        [GeneratedRegex("( xmlns(:\\w+)?)=\"([^\"]+)\"")]
        private static partial Regex XmlNamespaceRegex();
    }
}
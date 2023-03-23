using System;
using System.Collections.Generic;

namespace PrivateGalleryCreator
{
    public class Package
    {
        public Package(string fileName, string fullSourcePath)
        {
            FileName = fileName;
            FullPath = fullSourcePath;
        }

        public string FileName { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string DevVersion { get; set; }
        public string Icon { get; set; }
        public string Preview { get; set; }
        public string Tags { get; set; }
        public DateTime DatePublished { get; set; }
        public IEnumerable<string> SupportedVersions { get; set; }
        public string License { get; set; }
        public string GettingStartedUrl { get; set; }
        public string ReleaseNotesUrl { get; set; }
        public string MoreInfoUrl { get; set; }
		public string Repo { get; set; }
		public string IssueTracker { get; set; }
        public ExtensionList ExtensionList { get; set; }
        public string FullPath { get; set; }

		public override string ToString()
		{
			return Name;
		}
    }
}
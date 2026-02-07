using System;
using System.Collections.Generic;

namespace PrivateGalleryCreator
{
    /// <summary>
    /// Represents a single VSIX installation target (e.g. VS Community, Pro, Enterprise) with its version range.
    /// </summary>
    public record InstallationTarget(string Identifier, string VersionRange);

    public class Package(string fileName, string fullSourcePath)
    {
        public string FileName { get; set; } = fileName;
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
        public IEnumerable<InstallationTarget> InstallationTargets { get; set; }
        public string License { get; set; }
        public string GettingStartedUrl { get; set; }
        public string ReleaseNotesUrl { get; set; }
        public string MoreInfoUrl { get; set; }
        public string Repo { get; set; }
        public string IssueTracker { get; set; }
        public ExtensionList ExtensionList { get; set; }
        public string FullPath { get; set; } = fullSourcePath;

        public override string ToString()
        {
            return Name;
        }
    }
}
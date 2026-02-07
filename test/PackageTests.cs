using Xunit;

namespace PrivateGalleryCreator.Tests;

public class PackageTests
{
    [Fact]
    public void Constructor_SetsFileName()
    {
        var package = new Package("extension.vsix", "/path/to/extension.vsix");

        Assert.Equal("extension.vsix", package.FileName);
    }

    [Fact]
    public void Constructor_SetsFullPath()
    {
        var package = new Package("extension.vsix", "/path/to/extension.vsix");

        Assert.Equal("/path/to/extension.vsix", package.FullPath);
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        var package = new Package("extension.vsix", "/path/to/extension.vsix")
        {
            Name = "My Extension"
        };

        Assert.Equal("My Extension", package.ToString());
    }

    [Fact]
    public void ToString_WhenNameIsNull_ReturnsNull()
    {
        var package = new Package("extension.vsix", "/path/to/extension.vsix");

        Assert.Null(package.ToString());
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("2.5.3")]
    [InlineData("10.0.0.1")]
    public void Version_AcceptsValidVersionStrings(string version)
    {
        var package = new Package("extension.vsix", "/path/to/extension.vsix")
        {
            Version = version
        };

        Assert.Equal(version, package.Version);
    }

    [Fact]
    public void AllProperties_CanBeSetAndRetrieved()
    {
        var datePublished = DateTime.UtcNow;
        var installationTargets = new[] {
            new InstallationTarget("Microsoft.VisualStudio.Community", "[17.0,18.0)", "amd64"),
            new InstallationTarget("Microsoft.VisualStudio.Pro", "[17.0,18.0)", "arm64")
        };
        var extensionList = new ExtensionList
        {
            ID = "list-id",
            Name = "Test List",
            Extensions = [new Extension { VsixId = "ext1", Name = "Extension 1" }]
        };

        var package = new Package("test.vsix", "/full/path/test.vsix")
        {
            ID = "test-id",
            Name = "Test Package",
            Description = "Test description",
            Author = "Test Author",
            Version = "1.2.3",
            DevVersion = "17.0",
            Icon = "icon.png",
            Preview = "preview.png",
            Tags = "tag1,tag2",
            DatePublished = datePublished,
            InstallationTargets = installationTargets,
            License = "MIT License",
            GettingStartedUrl = "https://example.com/start",
            ReleaseNotesUrl = "https://example.com/release",
            MoreInfoUrl = "https://example.com/info",
            Repo = "https://github.com/test/repo",
            IssueTracker = "https://github.com/test/repo/issues",
            ExtensionList = extensionList
        };

        Assert.Equal("test.vsix", package.FileName);
        Assert.Equal("/full/path/test.vsix", package.FullPath);
        Assert.Equal("test-id", package.ID);
        Assert.Equal("Test Package", package.Name);
        Assert.Equal("Test description", package.Description);
        Assert.Equal("Test Author", package.Author);
        Assert.Equal("1.2.3", package.Version);
        Assert.Equal("17.0", package.DevVersion);
        Assert.Equal("icon.png", package.Icon);
        Assert.Equal("preview.png", package.Preview);
        Assert.Equal("tag1,tag2", package.Tags);
        Assert.Equal(datePublished, package.DatePublished);
        Assert.Equal(installationTargets, package.InstallationTargets);
        Assert.Equal("MIT License", package.License);
        Assert.Equal("https://example.com/start", package.GettingStartedUrl);
        Assert.Equal("https://example.com/release", package.ReleaseNotesUrl);
        Assert.Equal("https://example.com/info", package.MoreInfoUrl);
        Assert.Equal("https://github.com/test/repo", package.Repo);
        Assert.Equal("https://github.com/test/repo/issues", package.IssueTracker);
        Assert.Same(extensionList, package.ExtensionList);
    }
}

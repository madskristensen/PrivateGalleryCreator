using System.Xml.Linq;

using Xunit;

namespace PrivateGalleryCreator.Tests;

public class FeedWriterTests
{
    private readonly FeedWriter _feedWriter;

    public FeedWriterTests()
    {
        _feedWriter = new FeedWriter("Test Gallery");
    }

    [Fact]
    public void GetFeed_WithEmptyPackages_ReturnsValidAtomFeed()
    {
        string result = _feedWriter.GetFeed("feed.xml", []);

        XDocument doc = XDocument.Parse(result);
        XNamespace atom = "http://www.w3.org/2005/Atom";

        Assert.NotNull(doc.Root);
        Assert.Equal("feed", doc.Root.Name.LocalName);
        Assert.Equal(atom, doc.Root.Name.Namespace);
    }

    [Fact]
    public void GetFeed_ContainsCorrectGalleryTitle()
    {
        string result = _feedWriter.GetFeed("feed.xml", []);

        XDocument doc = XDocument.Parse(result);
        XNamespace atom = "http://www.w3.org/2005/Atom";
        string? title = doc.Root?.Element(atom + "title")?.Value;

        Assert.Equal("Test Gallery", title);
    }

    [Fact]
    public void GetFeed_ContainsSubtitle()
    {
        string result = _feedWriter.GetFeed("feed.xml", []);

        XDocument doc = XDocument.Parse(result);
        XNamespace atom = "http://www.w3.org/2005/Atom";
        string? subtitle = doc.Root?.Element(atom + "subtitle")?.Value;

        Assert.Contains("Visual Studio", subtitle);
    }

    [Fact]
    public void GetFeed_WithSinglePackage_CreatesEntry()
    {
        var package = CreateTestPackage();

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        XDocument doc = XDocument.Parse(result);
        XNamespace atom = "http://www.w3.org/2005/Atom";
        var entries = doc.Root?.Elements(atom + "entry").ToList();

        Assert.NotNull(entries);
        Assert.Single(entries);
    }

    [Fact]
    public void GetFeed_PackageEntry_ContainsCorrectId()
    {
        var package = CreateTestPackage();
        package.ID = "TestExtension.12345";

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        XDocument doc = XDocument.Parse(result);
        XNamespace atom = "http://www.w3.org/2005/Atom";
        var entry = doc.Root?.Element(atom + "entry");
        string? id = entry?.Element(atom + "id")?.Value;

        Assert.Equal("TestExtension.12345", id);
    }

    [Fact]
    public void GetFeed_PackageEntry_ContainsCorrectTitle()
    {
        var package = CreateTestPackage();
        package.Name = "My Test Extension";

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        XDocument doc = XDocument.Parse(result);
        XNamespace atom = "http://www.w3.org/2005/Atom";
        var entry = doc.Root?.Element(atom + "entry");
        string? title = entry?.Element(atom + "title")?.Value;

        Assert.Equal("My Test Extension", title);
    }

    [Fact]
    public void GetFeed_PackageEntry_ContainsAuthor()
    {
        var package = CreateTestPackage();
        package.Author = "Test Author";

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        XDocument doc = XDocument.Parse(result);
        XNamespace atom = "http://www.w3.org/2005/Atom";
        var entry = doc.Root?.Element(atom + "entry");
        string? author = entry?.Element(atom + "author")?.Element(atom + "name")?.Value;

        Assert.Equal("Test Author", author);
    }

    [Fact]
    public void GetFeed_WithMultiplePackages_CreatesMultipleEntries()
    {
        var packages = new[]
        {
            CreateTestPackage("Package1", "1.0.0"),
            CreateTestPackage("Package2", "2.0.0"),
            CreateTestPackage("Package3", "3.0.0")
        };

        string result = _feedWriter.GetFeed("feed.xml", packages);

        XDocument doc = XDocument.Parse(result);
        XNamespace atom = "http://www.w3.org/2005/Atom";
        var entries = doc.Root?.Elements(atom + "entry").ToList();

        Assert.NotNull(entries);
        Assert.Equal(3, entries.Count);
    }

    [Fact]
    public void GetFeed_OutputContainsUtf8Declaration()
    {
        string result = _feedWriter.GetFeed("feed.xml", []);

        Assert.Contains("utf-8", result);
        Assert.DoesNotContain("utf-16", result);
    }

    [Fact]
    public void GetFeed_PackageWithIcon_IncludesIconLink()
    {
        var package = CreateTestPackage();
        package.Icon = "Resources/icon.png";

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        Assert.Contains("rel=\"icon\"", result);
        Assert.Contains(".png", result);
    }

    [Fact]
    public void GetFeed_PackageWithoutIcon_DoesNotIncludeIconLink()
    {
        var package = CreateTestPackage();
        package.Icon = null;

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        Assert.DoesNotContain("rel=\"icon\"", result);
    }

    [Fact]
    public void GetFeed_PackageWithMoreInfoUrl_IncludesMoreInfoElement()
    {
        var package = CreateTestPackage();
        package.MoreInfoUrl = "https://example.com/info";

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        Assert.Contains("<MoreInfo>https://example.com/info</MoreInfo>", result);
    }

    [Fact]
    public void GetFeed_PackageWithReleaseNotesUrl_IncludesReleaseNotesElement()
    {
        var package = CreateTestPackage();
        package.ReleaseNotesUrl = "https://example.com/release-notes";

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        Assert.Contains("<ReleaseNotes>https://example.com/release-notes</ReleaseNotes>", result);
    }

    [Fact]
    public void GetFeed_ContainsVsixElements()
    {
        var package = CreateTestPackage();

        string result = _feedWriter.GetFeed("feed.xml", [package]);

        Assert.Contains("<Vsix", result);
        Assert.Contains("</Vsix>", result);
        Assert.Contains("<Id>", result);
        Assert.Contains("<Version>", result);
    }

    private static Package CreateTestPackage(string name = "TestPackage", string version = "1.0.0")
    {
        return new Package("test.vsix", "/path/to/test.vsix")
        {
            ID = $"{name}.{Guid.NewGuid():N}",
            Name = name,
            Description = "Test description",
            Author = "Test Author",
            Version = version,
            DevVersion = "17.0",
            DatePublished = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };
    }
}

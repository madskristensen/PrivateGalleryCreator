using System.Diagnostics;
using System.Xml.Linq;

using Xunit;

namespace PrivateGalleryCreator.Tests;

[Trait("Category", "Integration")]
public class IntegrationTests : IDisposable
{
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace Vsix = "http://schemas.microsoft.com/developer/vsx-syndication-schema/2010";

    private readonly string _testAssetsDir;
    private readonly string _exePath;
    private readonly string _workDir;

    public IntegrationTests()
    {
        string testOutputDir = AppContext.BaseDirectory;

        _testAssetsDir = Path.Combine(testOutputDir, "TestAssets");
        _exePath = Path.Combine(testOutputDir, "..", "..", "..", "..",
            "src", "bin", "Debug", "net10.0", "PrivateGalleryCreator.exe");
        _exePath = Path.GetFullPath(_exePath);

        _workDir = Path.Combine(Path.GetTempPath(), $"PGC_IntegrationTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_workDir))
        {
            Directory.Delete(_workDir, recursive: true);
        }
    }

    [Fact]
    public void BasicFeedGeneration_ProducesValidAtomWithAllEntries()
    {
        CopyAllVsixToWorkDir();
        string feedPath = Path.Combine(_workDir, "feed.xml");

        int exitCode = RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" -t");

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(feedPath), "feed.xml was not created");

        XDocument doc = XDocument.Load(feedPath);
        Assert.Equal("feed", doc.Root!.Name.LocalName);
        Assert.Equal(Atom, doc.Root.Name.Namespace);

        var entries = doc.Root.Elements(Atom + "entry").ToList();
        Assert.Equal(5, entries.Count);

        var ids = entries.Select(e => e.Element(Atom + "id")?.Value).OrderBy(id => id).ToList();
        Assert.Contains(ids, id => id!.Contains("CommentsVS"));
        Assert.Contains(ids, id => id!.Contains("InstaSearch"));
        Assert.Contains(ids, id => id!.Contains("MarkdownLintVS"));
        Assert.Contains(ids, id => id!.Contains("c38f3512-4653-4d97-a4c5-860a425209f6"));
    }

    [Fact]
    public void NameOption_SetsCustomGalleryTitle()
    {
        CopyAllVsixToWorkDir();
        string feedPath = Path.Combine(_workDir, "feed.xml");

        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" --name=\"My Custom Gallery\" -t");

        XDocument doc = XDocument.Load(feedPath);
        string? title = doc.Root?.Element(Atom + "title")?.Value;

        Assert.Equal("My Custom Gallery", title);
    }

    [Fact]
    public void ExcludeOption_FiltersOutMatchingFiles()
    {
        CopyAllVsixToWorkDir();
        string feedPath = Path.Combine(_workDir, "feed.xml");

        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" --exclude=Comment -t");

        XDocument doc = XDocument.Load(feedPath);
        var entries = doc.Root!.Elements(Atom + "entry").ToList();

        Assert.Equal(4, entries.Count);
        Assert.DoesNotContain(entries, e => e.Element(Atom + "id")?.Value?.Contains("CommentsVS") == true);
    }

    [Fact]
    public void RecursiveOption_FindsVsixInSubdirectories()
    {
        string subDir = Path.Combine(_workDir, "subdir");
        Directory.CreateDirectory(subDir);

        // Put 3 files in root, 2 in subdirectory
        CopyVsixToDir("Comment Studio v1.0.148.vsix", _workDir);
        CopyVsixToDir("Insta Search v1.0.35.vsix", _workDir);
        CopyVsixToDir("Markdown Lint v1.0.56.vsix", _workDir);
        CopyVsixToDir("KnownMonikers Explorer (64 bit) v1.5.20.vsix", subDir);
        CopyVsixToDir("KnownMonikers Explorer (64 bit) v1.5.21.vsix", subDir);

        string feedPath = Path.Combine(_workDir, "feed.xml");

        // Without --recursive, only the 3 root files should be found
        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" -t");
        XDocument doc = XDocument.Load(feedPath);
        Assert.Equal(3, doc.Root!.Elements(Atom + "entry").Count());

        // With --recursive, all 5 should be found
        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" --recursive -t");
        doc = XDocument.Load(feedPath);
        Assert.Equal(5, doc.Root!.Elements(Atom + "entry").Count());
    }

    [Fact]
    public void SourceOption_CompletesWithoutError()
    {
        CopyAllVsixToWorkDir();
        string feedPath = Path.Combine(_workDir, "feed.xml");

        int exitCode = RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" --source=https://example.com/vsix/ -t");

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(feedPath), "feed.xml was not created");

        XDocument doc = XDocument.Load(feedPath);
        var entries = doc.Root!.Elements(Atom + "entry").ToList();
        Assert.Equal(5, entries.Count);
    }

    /// <summary>
    /// BUG: --source sets Package.FullPath but FeedWriter writes Package.FileName
    /// to content/@src and link/@href, so the source URL never appears in the feed.
    /// This test documents the expected behavior per the README.
    /// </summary>
    [Fact(Skip = "Known bug: --source does not affect feed output. FeedWriter uses Package.FileName instead of Package.FullPath.")]
    public void SourceOption_ShouldOverrideDownloadUrlsInFeed()
    {
        CopyAllVsixToWorkDir();
        string feedPath = Path.Combine(_workDir, "feed.xml");

        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" --source=https://example.com/vsix/ -t");

        XDocument doc = XDocument.Load(feedPath);
        var entries = doc.Root!.Elements(Atom + "entry").ToList();

        foreach (var entry in entries)
        {
            var contentSrc = entry.Element(Atom + "content")?.Attribute("src")?.Value;
            Assert.NotNull(contentSrc);
            Assert.StartsWith("https://example.com/vsix/", contentSrc);
        }
    }

    [Fact]
    public void IconExtraction_CreatesIconFiles()
    {
        CopyAllVsixToWorkDir();
        string feedPath = Path.Combine(_workDir, "feed.xml");

        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" -t");

        string iconsDir = Path.Combine(_workDir, "icons");
        Assert.True(Directory.Exists(iconsDir), "icons directory was not created");

        var iconFiles = Directory.GetFiles(iconsDir);
        Assert.True(iconFiles.Length > 0, "No icon files were extracted");
    }

    [Fact]
    public void LatestOnlyOption_KeepsOnlyHighestVersionPerExtension()
    {
        // Two real versions of KnownMonikers Explorer: v1.5.20 and v1.5.21 (same ID).
        // With --latest-only, only v1.5.21 should survive.
        CopyVsixToDir("KnownMonikers Explorer (64 bit) v1.5.20.vsix", _workDir);
        CopyVsixToDir("KnownMonikers Explorer (64 bit) v1.5.21.vsix", _workDir);

        string feedPath = Path.Combine(_workDir, "feed.xml");

        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" --latest-only -t");

        XDocument doc = XDocument.Load(feedPath);
        var entries = doc.Root!.Elements(Atom + "entry").ToList();

        Assert.Single(entries);

        var vsixElement = entries[0].Element(Vsix + "Vsix");
        Assert.Equal("1.5.22", vsixElement?.Element(Vsix + "Version")?.Value);
    }

    [Fact]
    public void LatestOnlyOption_WithoutFlag_KeepsBothVersions()
    {
        // Without --latest-only, both versions should appear.
        CopyVsixToDir("KnownMonikers Explorer (64 bit) v1.5.20.vsix", _workDir);
        CopyVsixToDir("KnownMonikers Explorer (64 bit) v1.5.21.vsix", _workDir);

        string feedPath = Path.Combine(_workDir, "feed.xml");

        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" -t");

        XDocument doc = XDocument.Load(feedPath);
        var entries = doc.Root!.Elements(Atom + "entry").ToList();

        Assert.Equal(2, entries.Count);

        var versions = entries
            .Select(e => e.Element(Vsix + "Vsix")?.Element(Vsix + "Version")?.Value)
            .OrderBy(v => v)
            .ToList();
        Assert.Equal("1.5.20", versions[0]);
        Assert.Equal("1.5.22", versions[1]);
    }

    [Fact]
    public void FeedEntries_ContainExpectedMetadata()
    {
        CopyAllVsixToWorkDir();
        string feedPath = Path.Combine(_workDir, "feed.xml");

        RunCreator($"--input=\"{_workDir}\" --output=\"{feedPath}\" -t");

        XDocument doc = XDocument.Load(feedPath);
        var entry = doc.Root!.Elements(Atom + "entry")
            .First(e => e.Element(Atom + "id")?.Value?.Contains("CommentsVS") == true);

        Assert.Equal("Comment Studio", entry.Element(Atom + "title")?.Value);
        Assert.Contains("Mads Kristensen", entry.Element(Atom + "author")?.Element(Atom + "name")?.Value);

        var vsixElement = entry.Element(Vsix + "Vsix");
        Assert.NotNull(vsixElement);
        Assert.Equal("1.0.148", vsixElement.Element(Vsix + "Version")?.Value);
    }

    /// <summary>
    /// --version affects how extension pack IDs are serialized in the Vsix/PackedExtensionIDs element.
    /// VS 17.x: each extension ID as a separate element. Other versions: semicolon-separated.
    /// This only applies to .vsix files containing .vsext extension pack manifests.
    /// The test assets are regular extensions (no .vsext), so --version has no visible effect here.
    /// This option is undocumented in the README.
    /// </summary>
    [Fact(Skip = "--version only affects extension packs (.vsext). Test assets are regular extensions. Needs .vsext test data.")]
    public void VersionOption_AffectsExtensionPackIdFormat()
    {
        // TODO: Add a test .vsix containing a .vsext file to verify:
        // --version=17.0 → multiple <PackedExtensionIDs> elements
        // --version=16.0 → single semicolon-separated <PackedExtensionIDs> element
        Assert.Fail("Needs .vsext test data to verify.");
    }

    private void CopyAllVsixToWorkDir()
    {
        foreach (string vsix in Directory.GetFiles(_testAssetsDir, "*.vsix"))
        {
            File.Copy(vsix, Path.Combine(_workDir, Path.GetFileName(vsix)));
        }
    }

    private void CopyVsixToDir(string fileName, string targetDir)
    {
        File.Copy(Path.Combine(_testAssetsDir, fileName), Path.Combine(targetDir, fileName));
    }

    private int RunCreator(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _exePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit(TimeSpan.FromSeconds(30));

        return process.ExitCode;
    }
}

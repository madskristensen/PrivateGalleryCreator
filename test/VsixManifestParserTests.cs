using Xunit;

namespace PrivateGalleryCreator.Tests;

public class VsixManifestParserTests : IDisposable
{
    private readonly VsixManifestParser _parser;
    private readonly string _tempFolder;

    public VsixManifestParserTests()
    {
        _parser = new VsixManifestParser();
        _tempFolder = Path.Combine(Path.GetTempPath(), $"VsixManifestParserTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempFolder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempFolder))
        {
            Directory.Delete(_tempFolder, recursive: true);
        }
    }

    #region VS2012 Format Tests

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesIdentityId()
    {
        string manifest = CreateVs2012Manifest(id: "MyExtension.12345");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("MyExtension.12345", result.ID);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesDisplayName()
    {
        string manifest = CreateVs2012Manifest(displayName: "My Extension Name");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("My Extension Name", result.Name);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesDescription()
    {
        string manifest = CreateVs2012Manifest(description: "A great extension");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("A great extension", result.Description);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesVersion()
    {
        string manifest = CreateVs2012Manifest(version: "2.5.3.0");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("2.5.3.0", result.Version);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesPublisher()
    {
        string manifest = CreateVs2012Manifest(publisher: "Test Publisher");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("Test Publisher", result.Author);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesOptionalIcon()
    {
        string manifest = CreateVs2012Manifest(icon: "Resources/icon.png");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("Resources/icon.png", result.Icon);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesOptionalPreviewImage()
    {
        string manifest = CreateVs2012Manifest(previewImage: "Resources/preview.png");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("Resources/preview.png", result.Preview);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesOptionalTags()
    {
        string manifest = CreateVs2012Manifest(tags: "editor,productivity,tools");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("editor,productivity,tools", result.Tags);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_SetsDatePublishedToUtcNow()
    {
        string manifest = CreateVs2012Manifest();
        WriteManifest(manifest);
        DateTime before = DateTime.UtcNow;

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        DateTime after = DateTime.UtcNow;
        Assert.InRange(result.DatePublished, before, after);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesMoreInfoUrl()
    {
        string manifest = CreateVs2012Manifest(moreInfoUrl: "https://example.com/info");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("https://example.com/info", result.MoreInfoUrl);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesReleaseNotesUrl()
    {
        string manifest = CreateVs2012Manifest(releaseNotesUrl: "https://example.com/release");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("https://example.com/release", result.ReleaseNotesUrl);
    }

    [Fact]
    public void CreateFromManifest_Vs2012Format_ParsesGettingStartedUrl()
    {
        string manifest = CreateVs2012Manifest(gettingStartedUrl: "https://example.com/start");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("https://example.com/start", result.GettingStartedUrl);
    }

    #endregion

    #region VS2010 Format Tests

    [Fact]
    public void CreateFromManifest_Vs2010Format_ParsesIdentifierId()
    {
        string manifest = CreateVs2010Manifest(id: "OldExtension.789");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("OldExtension.789", result.ID);
    }

    [Fact]
    public void CreateFromManifest_Vs2010Format_ParsesName()
    {
        string manifest = CreateVs2010Manifest(name: "Old Format Extension");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("Old Format Extension", result.Name);
    }

    [Fact]
    public void CreateFromManifest_Vs2010Format_ParsesDescription()
    {
        string manifest = CreateVs2010Manifest(description: "A VS2010 era extension");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("A VS2010 era extension", result.Description);
    }

    [Fact]
    public void CreateFromManifest_Vs2010Format_ParsesVersion()
    {
        string manifest = CreateVs2010Manifest(version: "1.0.0.0");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("1.0.0.0", result.Version);
    }

    [Fact]
    public void CreateFromManifest_Vs2010Format_ParsesAuthor()
    {
        string manifest = CreateVs2010Manifest(author: "Legacy Author");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("Legacy Author", result.Author);
    }

    #endregion

    #region Supported Versions Tests

    [Fact]
    public void CreateFromManifest_ParsesSingleInstallationTarget()
    {
        string manifest = CreateVs2012Manifest(installationTarget: "[17.0,18.0)");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        var target = Assert.Single(result.InstallationTargets);
        Assert.Equal("Microsoft.VisualStudio.Community", target.Identifier);
        Assert.Equal("[17.0,18.0)", target.VersionRange);
        Assert.Equal("amd64", target.ProductArchitecture);
    }

    [Fact]
    public void CreateFromManifest_ParsesMultipleInstallationTargets()
    {
        string manifest = CreateVs2012ManifestWithMultipleTargets();
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal(2, result.InstallationTargets.Count());
        Assert.Contains(result.InstallationTargets, t => t.Identifier == "Microsoft.VisualStudio.Community" && t.VersionRange == "[16.0,17.0)" && t.ProductArchitecture == "amd64");
        Assert.Contains(result.InstallationTargets, t => t.Identifier == "Microsoft.VisualStudio.Pro" && t.VersionRange == "[17.0,18.0)" && t.ProductArchitecture == "arm64");
    }

    [Fact]
    public void CreateFromManifest_WhenNoInstallationTargets_ReturnsEmptyList()
    {
        string manifest = """
            <?xml version="1.0" encoding="utf-8"?>
            <PackageManifest>
                <Metadata>
                    <Identity Id="Test.Extension" Version="1.0.0" Publisher="Test" />
                    <DisplayName>Test Extension</DisplayName>
                    <Description>Test Description</Description>
                </Metadata>
                <Installation />
            </PackageManifest>
            """;
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Empty(result.InstallationTargets);
    }

    [Fact]
    public void CreateFromManifest_WhenInstallationTargetHasNoArchitecture_ArchitectureIsNull()
    {
        string manifest = """
            <?xml version="1.0" encoding="utf-8"?>
            <PackageManifest>
                <Metadata>
                    <Identity Id="Test.Extension" Version="1.0.0" Publisher="Test" />
                    <DisplayName>Test Extension</DisplayName>
                    <Description>Test Description</Description>
                </Metadata>
                <Installation>
                    <InstallationTarget Id="Microsoft.VisualStudio.Community" Version="[17.0,18.0)" />
                </Installation>
            </PackageManifest>
            """;
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        var target = Assert.Single(result.InstallationTargets);
        Assert.Equal("Microsoft.VisualStudio.Community", target.Identifier);
        Assert.Equal("[17.0,18.0)", target.VersionRange);
        Assert.Null(target.ProductArchitecture);
    }

    #endregion

    #region License Tests

    [Fact]
    public void CreateFromManifest_WhenLicenseFileExists_ReadsLicenseContent()
    {
        string manifest = CreateVs2012Manifest(license: "LICENSE.txt");
        WriteManifest(manifest);
        string licenseContent = "MIT License - Test Content";
        File.WriteAllText(Path.Combine(_tempFolder, "LICENSE.txt"), licenseContent);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal(licenseContent, result.License);
    }

    [Fact]
    public void CreateFromManifest_WhenLicenseFileDoesNotExist_LicenseIsNull()
    {
        string manifest = CreateVs2012Manifest(license: "NonExistent.txt");
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Null(result.License);
    }

    [Fact]
    public void CreateFromManifest_WhenNoLicenseElement_LicenseIsNull()
    {
        string manifest = CreateVs2012Manifest();
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Null(result.License);
    }

    #endregion

    #region Extension List Tests

    [Fact]
    public void CreateFromManifest_WhenVsextFileExists_ParsesExtensionList()
    {
        string manifest = CreateVs2012Manifest();
        WriteManifest(manifest);
        string vsextContent = """
            {
                "id": "ext-pack-id",
                "name": "Extension Pack",
                "version": "1.0.0",
                "extensions": [
                    { "vsixId": "ext1", "name": "Extension 1" }
                ]
            }
            """;
        File.WriteAllText(Path.Combine(_tempFolder, "extensions.vsext"), vsextContent);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.NotNull(result.ExtensionList);
        Assert.Equal("ext-pack-id", result.ExtensionList.ID);
        Assert.Equal("Extension Pack", result.ExtensionList.Name);
    }

    [Fact]
    public void CreateFromManifest_WhenNoVsextFile_ExtensionListIsNull()
    {
        string manifest = CreateVs2012Manifest();
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Null(result.ExtensionList);
    }

    #endregion

    #region Package Metadata Tests

    [Fact]
    public void CreateFromManifest_SetsCorrectFileName()
    {
        string manifest = CreateVs2012Manifest();
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "MyExtension.vsix", "/source/path");

        Assert.Equal("MyExtension.vsix", result.FileName);
    }

    [Fact]
    public void CreateFromManifest_SetsCorrectFullPath()
    {
        string manifest = CreateVs2012Manifest();
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/full/source/path/test.vsix");

        Assert.Equal("/full/source/path/test.vsix", result.FullPath);
    }

    #endregion

    #region Namespace Stripping Tests

    [Fact]
    public void CreateFromManifest_StripsXmlNamespaces()
    {
        string manifest = """
            <?xml version="1.0" encoding="utf-8"?>
            <PackageManifest xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011" xmlns:d="http://schemas.microsoft.com/developer/vsx-schema-design/2011">
                <Metadata>
                    <Identity Id="Test.Extension" Version="1.0.0" Publisher="Test" />
                    <DisplayName>Test Extension</DisplayName>
                    <Description>Test Description</Description>
                </Metadata>
                <Installation>
                    <InstallationTarget Id="Microsoft.VisualStudio.Community" Version="[17.0,18.0)" />
                </Installation>
            </PackageManifest>
            """;
        WriteManifest(manifest);

        Package result = VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix");

        Assert.Equal("Test.Extension", result.ID);
        Assert.Equal("Test Extension", result.Name);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void CreateFromManifest_WhenManifestFileNotFound_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix"));
    }

    [Fact]
    public void CreateFromManifest_WhenRequiredFieldMissing_ThrowsException()
    {
        string manifest = """
            <?xml version="1.0" encoding="utf-8"?>
            <PackageManifest>
                <Metadata>
                    <Identity Version="1.0.0" Publisher="Test" />
                    <DisplayName>Test</DisplayName>
                    <Description>Test</Description>
                </Metadata>
            </PackageManifest>
            """;
        WriteManifest(manifest);

        Exception ex = Assert.Throws<Exception>(() =>
            VsixManifestParser.CreateFromManifest(_tempFolder, "test.vsix", "/path/test.vsix"));

        Assert.Contains("Id", ex.Message);
    }

    #endregion

    #region Helper Methods

    private void WriteManifest(string content)
    {
        File.WriteAllText(Path.Combine(_tempFolder, "extension.vsixmanifest"), content);
    }

    private static string CreateVs2012Manifest(
        string id = "Test.Extension",
        string version = "1.0.0",
        string publisher = "Test Publisher",
        string displayName = "Test Extension",
        string description = "Test Description",
        string? icon = null,
        string? previewImage = null,
        string? tags = null,
        string? license = null,
        string? moreInfoUrl = null,
        string? releaseNotesUrl = null,
        string? gettingStartedUrl = null,
        string installationTarget = "[17.0,18.0)")
    {
        string iconElement = icon != null ? $"<Icon>{icon}</Icon>" : "";
        string previewElement = previewImage != null ? $"<PreviewImage>{previewImage}</PreviewImage>" : "";
        string tagsElement = tags != null ? $"<Tags>{tags}</Tags>" : "";
        string licenseElement = license != null ? $"<License>{license}</License>" : "";
        string moreInfoElement = moreInfoUrl != null ? $"<MoreInfo>{moreInfoUrl}</MoreInfo>" : "";
        string releaseNotesElement = releaseNotesUrl != null ? $"<ReleaseNotes>{releaseNotesUrl}</ReleaseNotes>" : "";
        string gettingStartedElement = gettingStartedUrl != null ? $"<GettingStartedGuide>{gettingStartedUrl}</GettingStartedGuide>" : "";

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <PackageManifest>
                <Metadata>
                    <Identity Id="{id}" Version="{version}" Publisher="{publisher}" />
                    <DisplayName>{displayName}</DisplayName>
                    <Description>{description}</Description>
                    {iconElement}
                    {previewElement}
                    {tagsElement}
                    {licenseElement}
                    {moreInfoElement}
                    {releaseNotesElement}
                    {gettingStartedElement}
                </Metadata>
                <Installation>
                    <InstallationTarget Id="Microsoft.VisualStudio.Community" Version="{installationTarget}">
                        <ProductArchitecture>amd64</ProductArchitecture>
                    </InstallationTarget>
                </Installation>
            </PackageManifest>
            """;
    }

    private static string CreateVs2012ManifestWithMultipleTargets()
    {
        return """
            <?xml version="1.0" encoding="utf-8"?>
            <PackageManifest>
                <Metadata>
                    <Identity Id="Test.Extension" Version="1.0.0" Publisher="Test" />
                    <DisplayName>Test Extension</DisplayName>
                    <Description>Test Description</Description>
                </Metadata>
                <Installation>
                    <InstallationTarget Id="Microsoft.VisualStudio.Community" Version="[16.0,17.0)">
                        <ProductArchitecture>amd64</ProductArchitecture>
                    </InstallationTarget>
                    <InstallationTarget Id="Microsoft.VisualStudio.Pro" Version="[17.0,18.0)">
                        <ProductArchitecture>arm64</ProductArchitecture>
                    </InstallationTarget>
                </Installation>
            </PackageManifest>
            """;
    }

    private static string CreateVs2010Manifest(
        string id = "Legacy.Extension",
        string version = "1.0.0.0",
        string author = "Legacy Author",
        string name = "Legacy Extension",
        string description = "Legacy Description")
    {
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <Vsix>
                <Identifier Id="{id}">
                    <Name>{name}</Name>
                    <Author>{author}</Author>
                    <Version>{version}</Version>
                    <Description>{description}</Description>
                    <SupportedFrameworkRuntimeEdition MinVersion="4.0" MaxVersion="4.5" />
                </Identifier>
                <References />
                <Content>
                    <VsPackage>MyPackage.pkgdef</VsPackage>
                </Content>
            </Vsix>
            """;
    }

    #endregion
}

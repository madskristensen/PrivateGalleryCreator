using System.Runtime.Serialization.Json;
using System.Text;

using Xunit;

namespace PrivateGalleryCreator.Tests;

public class ExtensionListTests
{
    [Fact]
    public void ExtensionList_ToString_ReturnsName()
    {
        var list = new ExtensionList { Name = "My Extension List" };

        Assert.Equal("My Extension List", list.ToString());
    }

    [Fact]
    public void Extension_ToString_ReturnsName()
    {
        var extension = new Extension { Name = "My Extension" };

        Assert.Equal("My Extension", extension.ToString());
    }

    [Fact]
    public void ExtensionList_CanDeserializeFromJson()
    {
        string json = """
            {
                "id": "test-list-id",
                "name": "Test Extension Pack",
                "version": "1.0.0",
                "extensions": [
                    { "vsixId": "ext-1", "name": "Extension One" },
                    { "vsixId": "ext-2", "name": "Extension Two" }
                ]
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(typeof(ExtensionList));
        var result = (ExtensionList?)serializer.ReadObject(stream);

        Assert.NotNull(result);
        Assert.Equal("test-list-id", result.ID);
        Assert.Equal("Test Extension Pack", result.Name);
        Assert.Equal("1.0.0", result.Version);
        Assert.NotNull(result.Extensions);
        Assert.Equal(2, result.Extensions.Length);
    }

    [Fact]
    public void ExtensionList_Extensions_HaveCorrectProperties()
    {
        string json = """
            {
                "id": "list-id",
                "name": "List",
                "version": "1.0.0",
                "extensions": [
                    { "vsixId": "test-vsix-id", "name": "Test Extension" }
                ]
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(typeof(ExtensionList));
        var result = (ExtensionList?)serializer.ReadObject(stream);

        Assert.NotNull(result?.Extensions);
        Assert.Single(result.Extensions);
        Assert.Equal("test-vsix-id", result.Extensions[0].VsixId);
        Assert.Equal("Test Extension", result.Extensions[0].Name);
    }

    [Fact]
    public void ExtensionList_CanDeserializeEmptyExtensions()
    {
        string json = """
            {
                "id": "empty-list",
                "name": "Empty List",
                "version": "1.0.0",
                "extensions": []
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(typeof(ExtensionList));
        var result = (ExtensionList?)serializer.ReadObject(stream);

        Assert.NotNull(result);
        Assert.NotNull(result.Extensions);
        Assert.Empty(result.Extensions);
    }

    [Fact]
    public void ExtensionList_CanDeserializeWithoutExtensionsProperty()
    {
        string json = """
            {
                "id": "minimal-list",
                "name": "Minimal List",
                "version": "1.0.0"
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(typeof(ExtensionList));
        var result = (ExtensionList?)serializer.ReadObject(stream);

        Assert.NotNull(result);
        Assert.Equal("minimal-list", result.ID);
        Assert.Null(result.Extensions);
    }

    [Fact]
    public void ExtensionList_AllPropertiesCanBeSet()
    {
        var extensions = new[]
        {
            new Extension { VsixId = "id1", Name = "Ext1" },
            new Extension { VsixId = "id2", Name = "Ext2" }
        };

        var list = new ExtensionList
        {
            ID = "list-id",
            Name = "List Name",
            Version = "2.0.0",
            Extensions = extensions
        };

        Assert.Equal("list-id", list.ID);
        Assert.Equal("List Name", list.Name);
        Assert.Equal("2.0.0", list.Version);
        Assert.Same(extensions, list.Extensions);
    }

    [Fact]
    public void Extension_AllPropertiesCanBeSet()
    {
        var extension = new Extension
        {
            VsixId = "my-vsix-id",
            Name = "My Extension Name"
        };

        Assert.Equal("my-vsix-id", extension.VsixId);
        Assert.Equal("My Extension Name", extension.Name);
    }
}

using System;
using System.IO;
using System.Text;
using OpenSage.Data;
using Xunit;

namespace OpenSage.Tests.Data.InstallationLocator;

public class VdfParserTests
{
    public VdfParserTests()
    {
    }

    [Fact]
    public void CanParseEmptyDictionary()
    {
        var input = "\"root\" {}";
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        var parser = new VdfParser(reader);
        var result = parser.Parse();

        Assert.Single(result);
        Assert.True(result.ContainsKey("root"));
        Assert.NotNull(result["root"].DictionaryValue);
        Assert.Empty(result["root"].DictionaryValue!);
    }

    [Fact]
    public void CanParseStringValues()
    {
        var input = "\"key1\" \"value1\"\n\"key2\" \"value2\"";
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        var parser = new VdfParser(reader);
        var result = parser.Parse();

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"].StringValue);
        Assert.Equal("value2", result["key2"].StringValue);
    }

    [Fact]
    public void CanParseNestedDictionaries()
    {
        var input = "\"root\" { \"nested\" { \"key\" \"value\" } \"sibling\" \"value2\" }";
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        var parser = new VdfParser(reader);
        var result = parser.Parse();

        Assert.Single(result);
        var rootDict = result["root"].DictionaryValue;
        Assert.NotNull(rootDict);
        Assert.Equal(2, rootDict!.Count);

        var nestedDict = rootDict["nested"].DictionaryValue;
        Assert.NotNull(nestedDict);
        Assert.Single(nestedDict!);
        Assert.Equal("value", nestedDict["key"].StringValue);

        Assert.Equal("value2", rootDict["sibling"].StringValue);
    }

    [Fact]
    public void CanParseEscapeSequences()
    {
        var input = @"""key1"" ""value with \""quotes\""""";
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));

        var parser = new VdfParser(reader);
        var result = parser.Parse();

        Assert.Equal(1, result.Count);
        Assert.Equal(@"value with ""quotes""", result["key1"].StringValue);
    }

    [Fact]
    public void CanParseLibraryFoldersFile()
    {
        var testFilePath = Path.Combine(
            Environment.CurrentDirectory,
            "Data",
            "InstallationLocator",
            "Assets",
            "libraryfolders.vdf");

        Assert.True(File.Exists(testFilePath), $"Test file not found: {testFilePath}");

        using var reader = new StreamReader(testFilePath, Encoding.UTF8);
        var parser = new VdfParser(reader);
        var result = parser.Parse();

        Assert.Single(result);
        Assert.True(result.ContainsKey("libraryfolders"));

        var libraryFolders = result["libraryfolders"].ExpectDictionary("libraryfolders");
        Assert.Equal(6, libraryFolders.Count); // 0 through 5

        var library0 = libraryFolders["0"].ExpectDictionary("0");
        Assert.Equal(@"C:\Program Files (x86)\Steam", library0["path"].ExpectString("path"));

        var apps0 = library0["apps"].ExpectDictionary("apps");
        Assert.True(apps0.ContainsKey("70")); // Half-Life
        Assert.Equal("589603955", apps0["70"].ExpectString("70"));
    }

    [Theory]
    [InlineData("\"key\" \"value\" \"unclosed string")]
    [InlineData("\"key\" {")]
    [InlineData("\"key\" { \"nested\" \"value\" }}")] // Extra closing brace
    public void ThrowsOnInvalidVdf(string input)
    {
        var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));
        var parser = new VdfParser(reader);

        Assert.Throws<InvalidDataException>(() => parser.Parse());
    }
}

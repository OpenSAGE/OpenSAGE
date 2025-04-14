#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace OpenSage.Data.Map;

public sealed class AssetPropertyCollection : KeyedCollection<string, AssetProperty>
{
    internal static AssetPropertyCollection Parse(BinaryReader reader, MapParseContext context)
    {
        var numProperties = reader.ReadUInt16();
        var result = new AssetProperty[numProperties];

        for (var i = 0; i < numProperties; i++)
        {
            result[i] = AssetProperty.Parse(reader, context);
        }

        return new AssetPropertyCollection(result);
    }

    internal AssetPropertyCollection(IList<AssetProperty> list)
    {
        foreach (var property in list)
        {
            Add(property);
        }
    }

    internal AssetPropertyCollection() { }

    public void AddAsciiString(string key, string value)
    {
        Add(new AssetProperty(key, AssetPropertyType.AsciiString, value));
    }

    public void AddNullableAsciiString(string key, string? value)
    {
        if (value != null)
        {
            AddAsciiString(key, value);
        }
    }

    public void AddUnicodeString(string key, string value)
    {
        Add(new AssetProperty(key, AssetPropertyType.UnicodeString, value));
    }

    public void AddNullableUnicodeString(string key, string? value)
    {
        if (value != null)
        {
            AddUnicodeString(key, value);
        }
    }

    public void AddBoolean(string key, bool value)
    {
        Add(new AssetProperty(key, AssetPropertyType.Boolean, value));
    }

    public void AddNullableBoolean(string key, bool? value)
    {
        if (value.HasValue)
        {
            AddBoolean(key, value.Value);
        }
    }

    public void AddInteger(string key, int value)
    {
        Add(new AssetProperty(key, AssetPropertyType.Integer, value));
    }

    public void AddNullableInteger(string key, int? value)
    {
        if (value.HasValue)
        {
            AddInteger(key, value.Value);
        }
    }

    public void AddReal(string key, float value)
    {
        Add(new AssetProperty(key, AssetPropertyType.RealNumber, value));
    }

    public void AddNullableReal(string key, float? value)
    {
        if (value.HasValue)
        {
            AddReal(key, value.Value);
        }
    }

    // Set of add methods so that we can initialize this class with a collection initializer

    // TODO: This always creates an ascii string, does that actually matter?
    public void Add(string key, string value)
    {
        AddAsciiString(key, value);
    }

    public void Add(string key, bool value)
    {
        AddBoolean(key, value);
    }

    public void Add(string key, int value)
    {
        AddInteger(key, value);
    }

    public void Add(string key, float value)
    {
        AddReal(key, value);
    }

    public void Add(string key, AssetPropertyType type, object value)
    {
        Add(new AssetProperty(key, type, value));
    }

    protected override string GetKeyForItem(AssetProperty item)
    {
        return item.Key.Name;
    }

    public AssetProperty? GetPropOrNull(string key)
    {
        return TryGetValue(key, out var value) ? value : null;
    }

    internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
    {
        writer.Write((ushort)Count);

        foreach (var property in this)
        {
            property.WriteTo(writer, assetNames);
        }
    }
}

#nullable enable

using System;
using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map;

public sealed class AssetProperty
{
    public AssetPropertyKey Key { get; private set; }
    public object Value { get; private set; }

    internal static AssetProperty Parse(BinaryReader reader, MapParseContext context)
    {
        var key = AssetPropertyKey.Parse(reader, context);
        var value = key.PropertyType switch
        {
            AssetPropertyType.Boolean => reader.ReadBooleanChecked(),
            AssetPropertyType.Integer => reader.ReadInt32(),
            AssetPropertyType.RealNumber => (object)reader.ReadSingle(),
            AssetPropertyType.AsciiString => reader.ReadUInt16PrefixedAsciiString(),
            AssetPropertyType.Unknown => reader.ReadUInt16PrefixedAsciiString(),
            AssetPropertyType.UnicodeString => reader.ReadUInt16PrefixedUnicodeString(),
            _ => throw new InvalidDataException($"Unexpected property type: {key.PropertyType}."),
        };
        return new AssetProperty(key.Name, key.PropertyType, value);
    }

    internal AssetProperty(string name, AssetPropertyType propertyType, object value)
    {
        Key = new AssetPropertyKey(name, propertyType);
        Value = value;
    }

    internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
    {
        Key.WriteTo(writer, assetNames);

        switch (Key.PropertyType)
        {
            case AssetPropertyType.Boolean:
                writer.Write((bool)Value);
                break;

            case AssetPropertyType.Integer:
                writer.Write((int)Value);
                break;

            case AssetPropertyType.RealNumber:
                writer.Write((float)Value);
                break;

            case AssetPropertyType.AsciiString:
                writer.WriteUInt16PrefixedAsciiString((string)Value);
                break;

            case AssetPropertyType.Unknown:
                writer.WriteUInt16PrefixedAsciiString((string)Value);
                break;

            case AssetPropertyType.UnicodeString:
                writer.WriteUInt16PrefixedUnicodeString((string)Value);
                break;

            default:
                throw new InvalidDataException($"Unexpected property type: {Key.PropertyType}.");
        }
    }

    public override string ToString()
    {
        return $"{Key}: {Value}";
    }

    public string? GetAsciiString()
    {
        return Value as string;
    }

    public bool? GetBoolean()
    {
        return Value as bool?;
    }

    public int? GetInteger()
    {
        return Value as int?;
    }

    public float? GetReal()
    {
        return Value as float?;
    }

    public string? GetUnicodeString()
    {
        return Value as string;
    }

    public void UpdateValue(object newValue)
    {
        // Ensure the new value is of the same type as the original value
        if (newValue.GetType() != Value.GetType())
        {
            throw new InvalidOperationException($"Cannot update value. Expected type: {Value.GetType()}, but got: {newValue.GetType()}");
        }
        Value = newValue;
    }
}

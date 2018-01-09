using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class AssetProperty
    {
        public AssetPropertyKey Key { get; private set; }
        public object Value { get; private set; }

        internal static AssetProperty Parse(BinaryReader reader, MapParseContext context)
        {
            var key = AssetPropertyKey.Parse(reader, context);

            object value = null;
            switch (key.PropertyType)
            {
                case AssetPropertyType.Boolean:
                    value = reader.ReadBooleanChecked();
                    break;

                case AssetPropertyType.Integer:
                    value = reader.ReadUInt32();
                    break;

                case AssetPropertyType.RealNumber:
                    value = reader.ReadSingle();
                    break;

                case AssetPropertyType.AsciiString:
                    value = reader.ReadUInt16PrefixedAsciiString();
                    break;

                case AssetPropertyType.Unknown:
                    // Seems exactly the same as AsciiString?
                    value = reader.ReadUInt16PrefixedAsciiString();
                    break;

                case AssetPropertyType.UnicodeString:
                    value = reader.ReadUInt16PrefixedUnicodeString();
                    break;

                default:
                    throw new InvalidDataException($"Unexpected property type: {key.PropertyType}.");
            }

            return new AssetProperty
            {
                Key = key,
                Value = value
            };
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            Key.WriteTo(writer, assetNames);

            switch (Key.PropertyType)
            {
                case AssetPropertyType.Boolean:
                    writer.Write((bool) Value);
                    break;

                case AssetPropertyType.Integer:
                    writer.Write((uint) Value);
                    break;

                case AssetPropertyType.RealNumber:
                    writer.Write((float) Value);
                    break;

                case AssetPropertyType.AsciiString:
                    writer.WriteUInt16PrefixedAsciiString((string) Value);
                    break;

                case AssetPropertyType.UnicodeString:
                    writer.WriteUInt16PrefixedUnicodeString((string) Value);
                    break;

                default:
                    throw new InvalidDataException($"Unexpected property type: {Key.PropertyType}.");
            }
        }

        public override string ToString()
        {
            return $"{Key}: {Value}";
        }
    }
}

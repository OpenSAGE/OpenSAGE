using System;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class AssetProperty
    {
        public AssetPropertyType PropertyType { get; private set; }
        public string Name { get; private set; }
        public object Value { get; private set; }

        public static AssetProperty Parse(BinaryReader reader, MapParseContext context)
        {
            AssetPropertyType propertyType;

            // TODO: Remove this code once we've accounted for all property types.
            string errorMessage = null;
            var hasError = false;
            try
            {
                propertyType = reader.ReadByteAsEnum<AssetPropertyType>();
            }
            catch (Exception ex)
            {
                propertyType = default(AssetPropertyType);
                hasError = true;
                errorMessage = ex.Message;
            }

            var propertyNameIndex = reader.ReadUInt24();
            var propertyName = context.GetAssetName(propertyNameIndex);

            if (hasError)
            {
                throw new System.Exception($"Unknown property type for property name {propertyName} {errorMessage}");
            }

            object value = null;
            switch (propertyType)
            {
                case AssetPropertyType.Boolean:
                    value = reader.ReadBoolean();
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

                case AssetPropertyType.UnicodeString:
                    value = reader.ReadUInt16PrefixedUnicodeString();
                    break;

                default:
                    throw new InvalidDataException($"Unexpected property type: {propertyType}.");
            }

            return new AssetProperty
            {
                PropertyType = propertyType,
                Name = propertyName,
                Value = value
            };
        }
    }

    public enum AssetPropertyType : byte
    {
        Boolean = 0,
        Integer = 1,
        RealNumber = 2,
        AsciiString = 3,
        UnicodeString = 4
    }
}

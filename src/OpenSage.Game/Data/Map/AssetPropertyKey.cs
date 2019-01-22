using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class AssetPropertyKey
    {
        public AssetPropertyType PropertyType { get; private set; }
        public string Name { get; private set; }

        internal static AssetPropertyKey Parse(BinaryReader reader, MapParseContext context)
        {
            var propertyType = reader.ReadByteAsEnum<AssetPropertyType>();

            var propertyNameIndex = reader.ReadUInt24();
            var propertyName = context.GetAssetName(propertyNameIndex);

            return new AssetPropertyKey
            {
                PropertyType = propertyType,
                Name = propertyName
            };
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            writer.Write((byte) PropertyType);

            writer.WriteUInt24(assetNames.GetOrCreateAssetIndex(Name));
        }

        public override string ToString()
        {
            return $"{Name} ({PropertyType})";
        }
    }
}

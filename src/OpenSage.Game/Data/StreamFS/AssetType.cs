using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Dds;

namespace OpenSage.Data.StreamFS
{
    public static class AssetTypeCatalog
    {
        private static readonly Dictionary<uint, AssetType> _assetTypes;

        static AssetTypeCatalog()
        {
            var assetTypes = new AssetType[]
            {
                new TextureAssetType()
            };

            _assetTypes = assetTypes.ToDictionary(x => x.TypeId);
        }

        public static bool TryGetAssetType(uint typeId, out AssetType assetType)
        {
            return _assetTypes.TryGetValue(typeId, out assetType);
        }
    }

    public abstract class AssetType
    {
        public abstract uint TypeId { get; }

        public abstract object Parse(Asset asset, BinaryReader reader);
    }

    public sealed class TextureAssetType : AssetType
    {
        public override uint TypeId { get; } = 0x21E727DA;

        public override object Parse(Asset asset, BinaryReader reader)
        {
            var zero = reader.ReadUInt32();
            if (zero != 0)
            {
                throw new InvalidDataException();
            }

            var twelve = reader.ReadUInt32();
            if (twelve != 12)
            {
                throw new InvalidDataException();
            }

            var ddsLength = reader.ReadUInt32();

            return DdsFile.FromStream(reader.BaseStream);
        }
    }
}

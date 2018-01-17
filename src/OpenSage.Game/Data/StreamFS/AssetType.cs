using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Dds;

namespace OpenSage.Data.StreamFS
{
    // TODO: This is only valid if the same TypeIds mean the same thing across games.
    public enum AssetType : uint
    {
        Texture = 0x21E727DA
    }

    public static class AssetReaderCatalog
    {
        private static readonly Dictionary<AssetType, AssetReader> _assetReaders;

        static AssetReaderCatalog()
        {
            var assetReaders = new AssetReader[]
            {
                new TextureReader()
            };

            _assetReaders = assetReaders.ToDictionary(x => x.AssetType);
        }

        public static bool TryGetAssetReader(uint typeId, out AssetReader assetReader)
        {
            return _assetReaders.TryGetValue((AssetType) typeId, out assetReader);
        }
    }

    public abstract class AssetReader
    {
        public abstract AssetType AssetType { get; }

        public abstract object Parse(Asset asset, BinaryReader reader, uint[] relocations, AssetImport[] imports);
    }

    public sealed class TextureReader : AssetReader
    {
        public override AssetType AssetType => AssetType.Texture;

        public override object Parse(Asset asset, BinaryReader reader, uint[] relocations, AssetImport[] imports)
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

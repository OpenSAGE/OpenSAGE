using System.IO;
using OpenSage.Data.Dds;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class TextureReader : AssetReader
    {
        public override AssetType AssetType => AssetType.Texture;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            return null;

            var twelve = reader.ReadUInt32();
            if (twelve != 12)
            {
                throw new InvalidDataException();
            }

            var ddsLength = reader.ReadUInt32();

            var ddsFile = DdsFile.FromStream(reader.BaseStream);

            return context.GraphicsDevice.CreateStaticTexture2D(
                ddsFile.Header.Width,
                ddsFile.Header.Height,
                ddsFile.ArraySize,
                ddsFile.MipMaps,
                ddsFile.PixelFormat,
                ddsFile.Dimension == DdsTextureDimension.TextureCube);
        }
    }
}

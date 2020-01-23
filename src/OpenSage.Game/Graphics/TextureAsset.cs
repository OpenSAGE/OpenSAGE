using System.IO;
using OpenSage.Data.Dds;
using OpenSage.Data.StreamFS;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class TextureAsset : BaseAsset
    {
        internal static TextureAsset ParseAsset(BinaryReader reader, Asset asset, AssetParseContext context)
        {
            var twelve = reader.ReadUInt32();
            if (twelve != 12)
            {
                throw new InvalidDataException();
            }

            reader.ReadUInt32(); // Length

            var ddsFile = DdsFile.FromStream(reader.BaseStream);

            var result = context.GraphicsDevice.CreateStaticTexture2D(
                ddsFile.Header.Width,
                ddsFile.Header.Height,
                ddsFile.ArraySize,
                ddsFile.MipMaps,
                ddsFile.PixelFormat,
                ddsFile.Dimension == DdsTextureDimension.TextureCube);

            result.Name = asset.Name;

            return new TextureAsset(result, asset);
        }

        public Texture Texture { get; }

        internal TextureAsset(Texture texture, string name)
        {
            SetNameAndInstanceId("Texture", name);
            Texture = AddDisposable(texture);
        }

        internal TextureAsset(Texture texture, Asset asset)
        {
            SetNameAndInstanceId(asset);
            Texture = AddDisposable(texture);
        }

        public static implicit operator Texture(TextureAsset asset) => asset.Texture;
    }
}

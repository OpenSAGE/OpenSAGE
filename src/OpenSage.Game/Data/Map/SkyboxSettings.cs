using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class SkyboxSettings : Asset
    {
        public const string AssetName = "SkyboxSettings";

        public Vector3 Position { get; private set; }
        public float Scale { get; private set; }
        public float Rotation { get; private set; }
        public string TextureScheme { get; private set; }

        internal static SkyboxSettings Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version => new SkyboxSettings
                {
                    Position = reader.ReadVector3(),
                    Scale = reader.ReadSingle(),
                    Rotation = reader.ReadSingle(),
                    TextureScheme = reader.ReadUInt16PrefixedAsciiString()
                });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write(Position);
                writer.Write(Scale);
                writer.Write(Rotation);
                writer.WriteUInt16PrefixedAsciiString(TextureScheme);
            });
        }
    }
}

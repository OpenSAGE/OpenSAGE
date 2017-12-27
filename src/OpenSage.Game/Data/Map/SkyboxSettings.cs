using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class SkyboxSettings : Asset
    {
        public const string AssetName = "SkyboxSettings";

        public byte[] Unknown { get; private set; }

        internal static SkyboxSettings Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                // TODO
                return new SkyboxSettings
                {
                    Unknown = reader.ReadBytes(32)
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write(Unknown);
            });
        }
    }
}

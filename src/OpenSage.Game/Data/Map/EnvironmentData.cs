using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class EnvironmentData : Asset
    {
        public const string AssetName = "EnvironmentData";

        public float WaterMaxAlphaDepth { get; private set; }
        public float DeepWaterAlpha { get; private set; }

        public string MacroTexture { get; private set; }
        public string CloudTexture { get; private set; }

        internal static EnvironmentData Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var result = new EnvironmentData();

                if (version >= 3)
                {
                    result.WaterMaxAlphaDepth = reader.ReadSingle();
                    result.DeepWaterAlpha = reader.ReadSingle();
                }

                var unknown = reader.ReadByte();
                if (unknown != 0)
                {
                    throw new InvalidDataException();
                }

                result.MacroTexture = reader.ReadUInt16PrefixedAsciiString();
                result.CloudTexture = reader.ReadUInt16PrefixedAsciiString();

                return result;
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                if (Version >= 3)
                {
                    writer.Write(WaterMaxAlphaDepth);
                    writer.Write(DeepWaterAlpha);
                }

                writer.Write((byte) 0);

                writer.WriteUInt16PrefixedAsciiString(MacroTexture);
                writer.WriteUInt16PrefixedAsciiString(CloudTexture);
            });
        }
    }
}

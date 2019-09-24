using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class EnvironmentData : Asset
    {
        public const string AssetName = "EnvironmentData";

        public float WaterMaxAlphaDepth { get; private set; }
        public float DeepWaterAlpha { get; private set; }

        public bool Unknown { get; private set; }

        public string MacroTexture { get; private set; }
        public string CloudTexture { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public string UnknownTexture { get; private set; }

        [AddedIn(SageGame.Ra3Uprising)]
        public string UnknownTexture2 { get; private set; }

        internal static EnvironmentData Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, (version, endPosition) =>
            {
                var result = new EnvironmentData();

                if (version >= 3)
                {
                    result.WaterMaxAlphaDepth = reader.ReadSingle();
                    result.DeepWaterAlpha = reader.ReadSingle();
                }

                if (version < 5)
                {
                    result.Unknown = reader.ReadBooleanChecked();
                }

                result.MacroTexture = reader.ReadUInt16PrefixedAsciiString();
                result.CloudTexture = reader.ReadUInt16PrefixedAsciiString();

                if (version >= 4)
                {
                    result.UnknownTexture = reader.ReadUInt16PrefixedAsciiString();
                }

                // Both RA3 Uprising and C&C4 used v6 for this chunk, but RA3 Uprising had an extra texture here.
                if (version >= 6 && reader.BaseStream.Position < endPosition)
                {
                    result.UnknownTexture2 = reader.ReadUInt16PrefixedAsciiString();
                }

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

                if (Version < 5)
                {
                    writer.Write(Unknown);
                }

                writer.WriteUInt16PrefixedAsciiString(MacroTexture);
                writer.WriteUInt16PrefixedAsciiString(CloudTexture);

                if (Version >= 4)
                {
                    writer.WriteUInt16PrefixedAsciiString(UnknownTexture);
                }

                if (Version >= 6 && UnknownTexture2 != null)
                {
                    writer.WriteUInt16PrefixedAsciiString(UnknownTexture2);
                }
            });
        }
    }
}

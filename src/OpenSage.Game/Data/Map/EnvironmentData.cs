using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class EnvironmentData : Asset
    {
        public const string AssetName = "EnvironmentData";

        public string MacroTexture { get; private set; }
        public string CloudTexture { get; private set; }

        internal static EnvironmentData Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var unknown = reader.ReadByte();
                if (unknown != 0)
                {
                    throw new InvalidDataException();
                }

                var macroTexture = reader.ReadUInt16PrefixedAsciiString();
                var cloudTexture = reader.ReadUInt16PrefixedAsciiString();

                return new EnvironmentData
                {
                    MacroTexture = macroTexture,
                    CloudTexture = cloudTexture
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((byte) 0);

                writer.WriteUInt16PrefixedAsciiString(MacroTexture);
                writer.WriteUInt16PrefixedAsciiString(CloudTexture);
            });
        }
    }
}

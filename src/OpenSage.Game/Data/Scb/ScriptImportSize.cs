using System.IO;
using OpenSage.Data.Map;

namespace OpenSage.Data.Scb
{
    public sealed class ScriptImportSize : Asset
    {
        public const string AssetName = "ScriptImportSize";

        public uint Unknown1 { get; private set; }
        public uint Unknown2 { get; private set; }

        internal static ScriptImportSize Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                return new ScriptImportSize
                {
                    Unknown1 = reader.ReadUInt32(),
                    Unknown2 = reader.ReadUInt32()
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write(Unknown1);
                writer.Write(Unknown2);
            });
        }
    }
}

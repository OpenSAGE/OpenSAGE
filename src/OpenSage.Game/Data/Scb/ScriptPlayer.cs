using System.IO;
using OpenSage.Data.Map;
using OpenSage.FileFormats;

namespace OpenSage.Data.Scb
{
    public sealed class ScriptPlayer
    {
        public string Name { get; private set; }
        public AssetPropertyCollection Properties { get; private set; }

        internal static ScriptPlayer Parse(BinaryReader reader, MapParseContext context, bool parseProperties)
        {
            return new ScriptPlayer()
            {
                Name = reader.ReadUInt16PrefixedAsciiString(),
                Properties = parseProperties ? AssetPropertyCollection.Parse(reader, context) : null
            };
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            writer.WriteUInt16PrefixedAsciiString(Name);
            if (Properties != null)
            {
                Properties.WriteTo(writer, assetNames);
            }
        }
    }
}

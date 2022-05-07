using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class LibraryMaps : Asset
    {
        public const string AssetName = "LibraryMaps";

        public string[] Values { get; private init; }

        internal static LibraryMaps Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, _ =>
            {
                var values = new string[reader.ReadUInt32()];
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = reader.ReadUInt16PrefixedAsciiString();
                }
                
                return new LibraryMaps
                {
                    Values = values
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Values.Length);

                foreach (var value in Values)
                {
                    writer.WriteUInt16PrefixedAsciiString(value);
                }
            });
        }
    }
}

using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class MPPositionInfo : Asset
    {
        public const string AssetName = "MPPositionInfo";

        public bool IsHuman { get; private set; }
        public bool IsComputer { get; private set; }
        public bool LoadAIScript { get; private set; }
        public uint Team { get; private set; }
        public string[] SideRestrictions { get; private set; }

        internal static MPPositionInfo Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var result = new MPPositionInfo
                {
                    IsHuman = reader.ReadBooleanChecked(),
                    IsComputer = reader.ReadBooleanChecked()
                };

                if (version > 0)
                {
                    // I'm guessing about what 5 bytes are missing in version 0, compared to version 1.
                    result.LoadAIScript = reader.ReadBooleanChecked();
                }

                result.Team = reader.ReadUInt32();

                if (version > 0)
                {
                    result.SideRestrictions = new string[reader.ReadUInt32()];
                    for (var i = 0; i < result.SideRestrictions.Length; i++)
                    {
                        result.SideRestrictions[i] = reader.ReadUInt16PrefixedAsciiString();
                    }
                }

                return result;
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write(IsHuman);
                writer.Write(IsComputer);

                if (Version > 0)
                {
                    writer.Write(LoadAIScript);
                }

                writer.Write(Team);

                if (Version > 0)
                {
                    writer.Write((uint) SideRestrictions.Length);
                    foreach (var sideRestriction in SideRestrictions)
                    {
                        writer.WriteUInt16PrefixedAsciiString(sideRestriction);
                    }
                }
            });
        }
    }
}

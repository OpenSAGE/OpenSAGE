using System.IO;
using OpenZH.Data.Map;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Scb
{
    public sealed class ScriptsPlayers : Asset
    {
        public const string AssetName = "ScriptsPlayers";

        public uint Unknown1 { get; private set; }
        public ushort Unknown2 { get; private set; }

        public string[] PlayerNames { get; private set; }

        internal static ScriptsPlayers Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var unknown1 = 0u;
                if (version > 1)
                {
                    unknown1 = reader.ReadUInt32();
                    if (unknown1 != 0)
                    {
                        throw new InvalidDataException();
                    }
                }

                var numPlayers = reader.ReadUInt32();

                var unknown2 = reader.ReadUInt16();
                if (unknown2 != 0)
                {
                    throw new InvalidDataException();
                }

                var playerNames = new string[numPlayers - 1];
                for (var i = 0; i < playerNames.Length; i++)
                {
                    playerNames[i] = reader.ReadUInt16PrefixedAsciiString();
                }

                return new ScriptsPlayers
                {
                    Unknown1 = unknown1,
                    Unknown2 = unknown2,
                    PlayerNames = playerNames
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                if (Version > 1)
                {
                    writer.Write(Unknown1);
                }

                writer.Write((uint) PlayerNames.Length + 1);

                writer.Write(Unknown2);

                foreach (var playerName in PlayerNames)
                {
                    writer.WriteUInt16PrefixedAsciiString(playerName);
                }
            });
        }
    }
}

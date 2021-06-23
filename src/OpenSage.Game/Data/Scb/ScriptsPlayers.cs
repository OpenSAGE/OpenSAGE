using System.IO;
using OpenSage.Data.Map;
using OpenSage.FileFormats;

namespace OpenSage.Data.Scb
{
    public sealed class ScriptsPlayers : Asset
    {
        public const string AssetName = "ScriptsPlayers";

        public bool HasPlayerProperties { get; private set; }
        public ushort Unknown { get; private set; }

        public ScriptsPlayer[] Players { get; private set; }

        internal static ScriptsPlayers Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var hasPlayerProperties = false;
                if (version > 1)
                {
                    hasPlayerProperties = reader.ReadBooleanUInt32Checked();
                }

                var numPlayers = reader.ReadUInt32();

                ushort unknown = 0;
                if (version < 2)
                {
                    unknown = reader.ReadUInt16();
                    if (unknown != 0)
                    {
                        throw new InvalidDataException();
                    }
                }

                var scriptPlayers = new ScriptsPlayer[numPlayers];
                for (var i = 0; i < scriptPlayers.Length; i++)
                {
                    scriptPlayers[i] = ScriptsPlayer.Parse(reader, context, hasPlayerProperties);
                }

                return new ScriptsPlayers
                {
                    HasPlayerProperties = hasPlayerProperties,
                    Unknown = unknown,
                    Players = scriptPlayers
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                if (Version > 1)
                {
                    writer.WriteBooleanUInt32(HasPlayerProperties);
                }

                writer.Write((uint) Players.Length + 1);

                if (Version < 2)
                {
                    writer.Write(Unknown);
                }

                foreach (var player in Players)
                {
                    player.WriteTo(writer, assetNames);
                }
            });
        }
    }
}

using System.IO;
using OpenSage.Data.Map;
using OpenSage.FileFormats;

namespace OpenSage.Data.Scb
{
    public sealed class ScriptsPlayers : Asset
    {
        public const string AssetName = "ScriptsPlayers";

        public bool HasPlayerProperties { get; private set; }

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

                var scriptPlayers = new ScriptsPlayer[numPlayers];
                for (var i = 0; i < scriptPlayers.Length; i++)
                {
                    scriptPlayers[i] = ScriptsPlayer.Parse(reader, context, hasPlayerProperties);
                }

                return new ScriptsPlayers
                {
                    HasPlayerProperties = hasPlayerProperties,
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

                writer.Write((uint) Players.Length);

                foreach (var player in Players)
                {
                    player.WriteTo(writer, assetNames);
                }
            });
        }
    }
}

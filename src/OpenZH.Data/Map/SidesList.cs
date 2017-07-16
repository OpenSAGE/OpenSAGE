using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class SidesList : Asset
    {
        public const string AssetName = "SidesList";

        public Player[] Players { get; private set; }
        public Team[] Teams { get; private set; }
        public PlayerScriptsList PlayerScripts { get; private set; }

        internal static SidesList Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numPlayers = reader.ReadUInt32();
                var players = new Player[numPlayers];

                for (var i = 0; i < numPlayers; i++)
                {
                    players[i] = Player.Parse(reader, context);
                }

                var numTeams = reader.ReadUInt32();
                var teams = new Team[numTeams];

                for (var i = 0; i < numTeams; i++)
                {
                    teams[i] = Team.Parse(reader, context);
                }

                PlayerScriptsList playerScripts = null;

                ParseAssets(reader, context, assetName =>
                {
                    if (assetName != PlayerScriptsList.AssetName)
                    {
                        throw new InvalidDataException();
                    }

                    if (playerScripts != null)
                    {
                        throw new InvalidDataException();
                    }

                    playerScripts = PlayerScriptsList.Parse(reader, context);
                });

                return new SidesList
                {
                    Players = players,
                    Teams = teams,
                    PlayerScripts = playerScripts
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Players.Length);

                foreach (var player in Players)
                {
                    player.WriteTo(writer, assetNames);
                }

                writer.Write((uint) Teams.Length);

                foreach (var team in Teams)
                {
                    team.WriteTo(writer, assetNames);
                }

                if (PlayerScripts != null)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(PlayerScriptsList.AssetName));
                    PlayerScripts.WriteTo(writer, assetNames);
                }
            });
        }
    }
}

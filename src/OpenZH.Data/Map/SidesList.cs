using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class SidesList : Asset
    {
        public Player[] Players { get; private set; }
        public Team[] Teams { get; private set; }
        public PlayerScriptsList PlayerScripts { get; private set; }

        public static SidesList Parse(BinaryReader reader, MapParseContext context)
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
                    if (assetName != "PlayerScriptsList")
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
    }
}

using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class SidesList
    {
        public Player[] Players { get; private set; }
        public Team[] Teams { get; private set; }
        public PlayerScriptsList PlayerScripts { get; private set; }

        public static SidesList Parse(BinaryReader reader, string[] assetStrings)
        {
            var numPlayers = reader.ReadUInt32();
            var players = new Player[numPlayers];
            
            for (var i = 0; i < numPlayers; i++)
            {
                players[i] = Player.Parse(reader, assetStrings);
            }

            var numTeams = reader.ReadUInt32();
            var teams = new Team[numTeams];

            for (var i = 0; i < numTeams; i++)
            {
                teams[i] = Team.Parse(reader, assetStrings);
            }

            var playerScripts = PlayerScriptsList.Parse(reader, assetStrings);

            return new SidesList
            {
                Players = players,
                Teams = teams,
                PlayerScripts = playerScripts
            };
        }
    }
}

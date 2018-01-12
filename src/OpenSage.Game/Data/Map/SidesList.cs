using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class SidesList : Asset
    {
        public const string AssetName = "SidesList";

        public bool Unknown { get; private set; }

        public Player[] Players { get; private set; }
        public Team[] Teams { get; private set; }
        public PlayerScriptsList PlayerScripts { get; private set; }

        internal static SidesList Parse(BinaryReader reader, MapParseContext context, bool mapHasAssetList)
        {
            return ParseAsset(reader, context, version =>
            {
                var unknown = false;
                if (version >= 6)
                {
                    unknown = reader.ReadBooleanChecked();
                }

                var numPlayers = reader.ReadUInt32();
                var players = new Player[numPlayers];

                for (var i = 0; i < numPlayers; i++)
                {
                    players[i] = Player.Parse(reader, context, version, mapHasAssetList);
                }

                if (version >= 5)
                {
                    // Above version 5, teams and scripts are in separate top-level chunks.
                    return new SidesList
                    {
                        Unknown = unknown,
                        Players = players
                    };
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

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames, bool mapHasAssetList)
        {
            WriteAssetTo(writer, () =>
            {
                if (Version >= 6)
                {
                    writer.Write(Unknown);
                }

                writer.Write((uint) Players.Length);

                foreach (var player in Players)
                {
                    player.WriteTo(writer, assetNames, Version, mapHasAssetList);
                }

                if (Version >= 5)
                {
                    return;
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

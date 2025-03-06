#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Content.Translation;
using OpenSage.Data.Map;
using OpenSage.Data.Scb;
using OpenSage.FileFormats;
using OpenSage.Scripting;

namespace OpenSage.Logic.Map;

using Player = Data.Map.Player;
using Team = Data.Map.Team;

public sealed class SidesList : Asset, IPersistableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public const string AssetName = "SidesList";

    // TODO: What game was this added in?
    public bool Unknown { get; private set; }

    // C++ name: m_sides
    private List<Player> _players = [];
    public List<Player> Players { get => _players; private set => _players = value; }

    // C++ name: m_teamrec
    private List<Team> _teams = [];
    public List<Team> Teams { get => _teams; private set => _teams = value; }

    #region Skirmish state
    // This is a bit iffy - this is essentially UI state stored in the global sides list
    // We could consider deviating from the original implementation here

    // C++ name: m_skirmishSides
    private List<Player> _skirmishPlayers = [];
    public List<Player> SkirmishPlayers { get => _skirmishPlayers; private set => _skirmishPlayers = value; }

    // C++ name: m_skirmishTeamrec
    private List<Team> _skirmishTeams = [];
    public List<Team> SkirmishTeams { get => _skirmishTeams; private set => _skirmishTeams = value; }

    #endregion

    internal static SidesList Parse(BinaryReader reader, MapParseContext context, bool mapHasAssetList)
    {
        return ParseAsset(reader, context, version =>
        {
            // ZH uses version 3

            var unknown = false;
            if (version >= 6)
            {
                unknown = reader.ReadBooleanChecked();
            }

            var numPlayers = reader.ReadInt32();
            var players = new List<Player>(numPlayers);

            for (var i = 0; i < numPlayers; i++)
            {
                players.Add(Player.Parse(reader, context, version, mapHasAssetList));
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

            var teams = new List<Team>();

            if (version >= 2)
            {
                var numTeams = reader.ReadInt32();

                for (var i = 0; i < numTeams; i++)
                {
                    teams.Add(Team.Parse(reader, context));
                }
            }

            PlayerScriptsList? playerScripts = null;

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


            // Attach the player scripts to the players
            if (playerScripts != null)
            {
                for (var i = 0; i < players.Count; i++)
                {
                    players[i].Scripts = playerScripts.ScriptLists[i];
                }
            }

            var sidesList = new SidesList
            {
                Players = players,
                Teams = teams,
            };

            var validationResult = sidesList.ValidateSides();
            if (validationResult == ValidationResult.InvalidButFixed)
            {
                Logger.Warn("Sides list had to be cleaned up after parsing");
            }

            return sidesList;
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

            writer.Write((uint)Players.Count);

            foreach (var player in Players)
            {
                player.WriteTo(writer, assetNames, Version, mapHasAssetList);
            }

            if (Version >= 5)
            {
                return;
            }

            writer.Write((uint)Teams.Count);

            foreach (var team in Teams)
            {
                team.WriteTo(writer, assetNames);
            }

            // Gather scripts lists from players and serialize them
            var playerScripts = GetPlayerScriptsList();
            writer.Write(assetNames.GetOrCreateAssetIndex(PlayerScriptsList.AssetName));
            playerScripts.WriteTo(writer, assetNames);
        });

        // Port note: the original C++ calls ValidateSides here, but it looks like a copy paste error?
        // Writing the sides list should not modify the sides list
    }

    public void Persist(StatePersister persister)
    {
        // Port note: in Generals this method's equivalent contains the PlayerScriptsList xfer logic directly
        var playerScriptsList = GetPlayerScriptsList();
        playerScriptsList.Persist(persister);
    }

    private PlayerScriptsList GetPlayerScriptsList()
    {
        var playerScripts = new PlayerScriptsList
        {
            ScriptLists = new ScriptList[Players.Count]
        };

        for (var i = 0; i < Players.Count; i++)
        {
            playerScripts.ScriptLists[i] = Players[i].Scripts;
        }

        return playerScripts;
    }

    enum ValidationResult
    {
        Valid,
        InvalidButFixed,
        // If we don't want to throw inside the validation method in the future consider adding a third value here
    }

    // Port note: Original C++ version returns a bool, and confusingly "true" means "invalid"
    // We use an enum instead to make the return value more explicit
    private ValidationResult ValidateSides()
    {
        var modified = false;

        // Ensure we have at least one player, and at least one neutral player
        var neutral = Players.FirstOrDefault(p => p.Name.Length == 0);

        if (neutral == null)
        {
            AddPlayerByTemplate("");
            modified = true;
        }

        // Ensure every player has a proper "default team"
        foreach (var player in Players)
        {
            var playerTeamName = $"team{player.Name}";
            var playerTeam = Teams.FirstOrDefault(t => t.Name == playerTeamName);

            if (playerTeam != null)
            {
                // Make sure the team owner points back to the player
                if (playerTeam.Owner != player.Name)
                {
                    // TODO(Port): ZH crashes only in debug mode and silently fixes the problem in release mode, should we do the same?
                    throw new InvalidOperationException(
                        $"Team owner mismatch: team {playerTeam.Name} is owned by {playerTeam.Owner}, but should be owned by {player.Name}"
                    );
                }

                // Default teams are always singletons
                if (!playerTeam.IsSingleton)
                {
                    // TODO(Port): ZH crashes only in debug mode and silently fixes the problem in release mode, should we do the same?
                    throw new InvalidOperationException(
                        $"Team {playerTeam.Name} is not a singleton"
                    );
                }
            }
            else
            {
                Logger.Warn($"Player {player.Name} has no default team, creating one");

                var team = new Team
                {
                    Name = playerTeamName,
                    Owner = player.Name,
                    IsSingleton = true,
                };

                Teams.Add(team);
                modified = true;
            }

            // Ensure all teams have valid allies and enemies
            // Comment from the original C++ code:
            // "(note that owners can be teams or players, but allies / enemies can only be teams)"
            // The comment seems to be wrong in two ways:
            // - Only players can be team owners (see CTeamsDialog::isValidTeamOwner in ZH WorldBuilder)
            // - Allies / enemies can in fact only be players (see ValidateAllyEnemyList below)

            var newAllies = ValidateAllyEnemyList(player, player.Allies);
            if (newAllies != null)
            {
                Logger.Warn($"Player {player.Name} has invalid allies, replacing with {newAllies}");
                player.Allies = newAllies;
                modified = true;
            }

            var newEnemies = ValidateAllyEnemyList(player, player.Enemies);
            if (newEnemies != null)
            {
                Logger.Warn($"Player {player.Name} has invalid enemies, replacing with {newEnemies}");
                player.Enemies = newEnemies;
                modified = true;
            }
        }

        // Ensure there is no overlap between team names and player names
        // (if there is, the player wins and the team is dropped)
        var teamNames = Teams.Select(t => t.Name).ToHashSet();
        var playerNames = Players.Select(p => p.Name).ToHashSet();

        var overlappingNames = teamNames.Intersect(playerNames).ToList();
        foreach (var overlappingName in overlappingNames)
        {
            Logger.Warn($"Player and team name conflict: {overlappingName}, removing team");
            Teams.RemoveAll(t => t.Name == overlappingName);
            modified = true;
        }

        // Ensure each team has an owner
        foreach (var team in Teams)
        {
            if (team.Owner == null)
            {
                Logger.Warn($"Team {team.Name} has no owner, setting to neutral player");
                team.Owner = "";
                modified = true;
            }
        }

        return modified ? ValidationResult.InvalidButFixed : ValidationResult.Valid;
    }

    private string? ValidateAllyEnemyList(Player player, string playerList)
    {
        // Short-circuit for empty lists, not present in the original code
        if (string.IsNullOrWhiteSpace(playerList))
        {
            return null;
        }

        var playerNames = playerList.Split(' ');
        var newPlayerList = new List<string>();
        var modified = false;

        foreach (var otherPlayerName in playerNames)
        {
            if (otherPlayerName == player.Name)
            {
                Logger.Warn($"Player {player.Name} is listed as an ally or enemy of themselves, removing");
                modified = true;
                continue;
            }

            var otherPlayer = Players.FirstOrDefault(p => p.Name == otherPlayerName);

            if (otherPlayer == null)
            {
                Logger.Warn($"Player {player.Name} has unknown ally or enemy {otherPlayerName}, removing");
                modified = true;
                continue;
            }

            newPlayerList.Add(otherPlayerName);
        }

        if (modified)
        {
            return string.Join(' ', newPlayerList);
        }
        else
        {
            return null;
        }
    }

    // Port note: This is only used in the ValidateSides method
    // (+ in WorldBuilder, which we are not porting for now)
    // So there must be some other player creation mechanism in the original code
    private void AddPlayerByTemplate(string playerTemplateName)
    {
        string playerName;
        string playerDisplayName;
        var isHuman = false;

        // Empty player template name means neutral player
        if (playerTemplateName.Length == 0)
        {
            playerName = "";
            playerDisplayName = "Neutral";
        }
        else
        {
            playerName = "Plyr";

            if (playerTemplateName.StartsWith("Faction"))
            {
                // This removes the "Faction" prefix
                playerName += playerTemplateName[7..];
            }
            else
            {
                playerName += playerTemplateName;
            }

            playerDisplayName = playerName.Translate();
            isHuman = true;

            // Special case "civilian"
            if (playerName == "PlyrCivilian")
            {
                isHuman = false;
            }
        }

        var player = new Player
        {
            Name = playerName,
            IsHuman = isHuman,
            DisplayName = playerDisplayName,
            Faction = playerTemplateName,
        };

        Players.Add(player);

        var team = new Team
        {
            Name = $"team{player.Name}",
            Owner = player.Name,
            IsSingleton = true,
        };

        Teams.Add(team);
    }

    // TODO(Port): Does this really belong here?
    // I think this should be a method on Game or some other global class
    public void PrepareForMpOrSkirmish(Game game)
    {
        // Move teams from the global list to the skirmish list
        SkirmishTeams.Clear();
        SkirmishTeams.AddRange(Teams);
        Teams.Clear();

        // Copy all players to the skirmish list
        SkirmishPlayers.Clear();
        SkirmishPlayers.AddRange(Players);

        // Remove all players from the global list, except for the civilian player
        // And leave at least one player in the global list (why?)
        var playerIndex = 0;
        while (playerIndex < Players.Count && Players.Count > 1)
        {
            if (Players[playerIndex].Faction == "FactionCivilian")
            {
                playerIndex++;
                continue;
            }

            Players.RemoveAt(playerIndex);
        }

        // If any of the players have scripts, don't make any changes
        var gotScripts = Players.Any(p => p.Faction != "FactionCivilian" && p.Scripts.Scripts.Length > 0);
        if (gotScripts)
        {
            return;
        }

        // Otherwise, load standard skirmish scripts
        // And throw away the skirmish teams list we just created
        SkirmishTeams.Clear();

        var skirmishScriptsEntry = game.ContentManager.GetScriptEntry(@"Data\Scripts\SkirmishScripts.scb");
        using var stream = skirmishScriptsEntry.Open();
        var skirmishScripts = ScbFile.FromStream(stream);

        // Port note: In Generals parsing the SCB file actually modifies (the equivalent of) SkirmishTeams
        foreach (var team in skirmishScripts.Teams.Teams)
        {
            var owner = SkirmishPlayers.FirstOrDefault(p => p.Name == team.Owner);
            if (owner != null)
            {
                SkirmishTeams.Add(team);
            }
        }

        // For each player, find the corresponding script list from skirmish scripts and copy it
        foreach (var player in SkirmishPlayers)
        {
            var index = Array.FindIndex(skirmishScripts.ScriptsPlayers.Players, scriptsPlayer => scriptsPlayer.Name == player.Name);
            if (index == -1)
            {
                // TODO: Is this actually something we should warn about?
                Logger.Warn($"No skirmish scripts found for player {player.Name}");
                continue;
            }
            player.Scripts = skirmishScripts.PlayerScripts.ScriptLists[index];
        }
    }
}

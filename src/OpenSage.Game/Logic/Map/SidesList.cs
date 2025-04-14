#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Content.Translation;
using OpenSage.Data.Map;
using OpenSage.Data.Scb;
using OpenSage.FileFormats;
using OpenSage.Logic.AI;
using OpenSage.Scripting;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic.Map;

using Player = Data.Map.Player;
using Team = Data.Map.Team;

public sealed class SidesList : Asset, IPersistableObject
{
    public const int MaxPlayerCount = 16;

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public const string AssetName = "SidesList";

    // TODO: What game was this added in?
    public bool Unknown { get; private set; }

    private readonly Player[] _sides = [.. Enumerable.Range(0, MaxPlayerCount).Select(_ => new Player([]))];
    private int _numSides;
    public ArraySegment<Player> Players { get => new ArraySegment<Player>(_sides, 0, _numSides); }

    // C++ name: m_teamrec
    private List<Team> _teams = [];
    public List<Team> Teams { get => _teams; private set => _teams = value; }

    #region Skirmish state
    // This is a bit iffy - this is essentially UI state stored in the global sides list
    // We could consider deviating from the original implementation here

    // Create MaxPlayerCount players for skirmish
    private readonly Player[] _skirmishSides = [.. Enumerable.Range(0, MaxPlayerCount).Select(_ => new Player([]))];
    private int _numSkirmishSides;
    public ArraySegment<Player> SkirmishPlayers { get => new ArraySegment<Player>(_skirmishSides, 0, _numSkirmishSides); }

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
            var players = new List<Player>(MaxPlayerCount);

            for (var i = 0; i < numPlayers; i++)
            {
                players.Add(Player.Parse(reader, context, version, mapHasAssetList));
            }

            if (version >= 5)
            {
                // Above version 5, teams and scripts are in separate top-level chunks.
                var newSidesList = new SidesList
                {
                    Unknown = unknown,
                };
                Array.Copy(players.ToArray(), newSidesList._sides, numPlayers);
                newSidesList._numSides = numPlayers;
                return newSidesList;
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
                Teams = teams,
            };
            Array.Copy(players.ToArray(), sidesList._sides, numPlayers);
            sidesList._numSides = numPlayers;

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

    internal enum ValidationResult
    {
        Valid,
        InvalidButFixed,
        // If we don't want to throw inside the validation method in the future consider adding a third value here
    }

    // Port note: Original C++ version returns a bool, and confusingly "true" means "invalid"
    // We use an enum instead to make the return value more explicit
    internal ValidationResult ValidateSides()
    {
        var modified = false;

        // Ensure we have at least one player, and at least one neutral player
        var neutral = Players.FirstOrDefault(p => p.Name?.Length == 0);

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

                var teamProps = new AssetPropertyCollection();
                teamProps.AddAsciiString(TeamKeys.Name, playerTeamName);
                teamProps.AddAsciiString(TeamKeys.Owner, player.Name ?? "");
                teamProps.AddBoolean(TeamKeys.IsSingleton, true);

                Teams.Add(new Team { Properties = teamProps });
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

    private string? ValidateAllyEnemyList(Player player, string? playerList)
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

    public void AddSide(Player mapPlayer)
    {
        if (_numSides >= MaxPlayerCount)
        {
            throw new InvalidOperationException("Too many players");
        }

        _sides[_numSides] = mapPlayer;
        _numSides++;
    }

    public void AddSide(AssetPropertyCollection properties)
    {
        if (_numSides >= MaxPlayerCount)
        {
            throw new InvalidOperationException("Too many players");
        }

        _sides[_numSides].Init(properties);
        _numSides++;
    }

    public void AddTeam(Team mapTeam)
    {
        Teams.Add(mapTeam);
    }

    public void RemoveSide(int index)
    {
        // Silently ignore invalid index
        if (index < 0 || index >= _numSides)
        {
            return;
        }

        // Can't remove the last player
        if (_numSides == 1)
        {
            return;
        }

        // Shift all players towards 0 and clear the last one
        for (var i = index; i < _numSides - 1; i++)
        {
            _sides[i] = _sides[i + 1];
        }
        _sides[_numSides - 1].Clear();
        _numSides--;
    }

    public void RemoveTeam(int index)
    {
        Teams.RemoveAt(index);
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

        var player = new Player(new() {
            { PlayerKeys.Name, playerName },
            { PlayerKeys.IsHuman, isHuman },
            { PlayerKeys.DisplayName, playerDisplayName },
            { PlayerKeys.Faction, playerTemplateName },
        });

        AddSide(player);

        var team = new Team(new() {
            { TeamKeys.Name, $"team{player.Name}" },
            { TeamKeys.Owner, player.Name ?? "" },
            { TeamKeys.IsSingleton, true }
        });

        AddTeam(team);
    }

    // Ported from GameLogic::startNewGame
    internal void AddObserverPlayer(IGame game)
    {
        var playerTemplate = game.AssetStore.PlayerTemplates.GetByName("FactionObserver");

        if (playerTemplate == null)
        {
            throw new InvalidOperationException("Observer player template not found");
        }

        var observerPlayer = new Player(new() {
            { PlayerKeys.Name, "ReplayObserver" },
            { PlayerKeys.IsHuman, true },
            { PlayerKeys.DisplayName, "Observer" },
            { PlayerKeys.Faction, playerTemplate.Name },
            { PlayerKeys.Allies, "" },
            { PlayerKeys.Enemies, "" },
            { PlayerKeys.Color, 0 },
            { PlayerKeys.NightColor, 0},
            { PlayerKeys.MultiplayerStartIndex, 0},
            { PlayerKeys.MultiplayerIsLocal, false },
        });

        AddSide(observerPlayer);

        var team = new Team(new() {
            { TeamKeys.Name, "teamReplayObserver" },
            { TeamKeys.Owner, "ReplayObserver" },
            { TeamKeys.IsSingleton, true }
        });

        AddTeam(team);
    }

    // TODO(Port): Does this really belong here?
    // I think this should be a method on Game or some other global class
    public void PrepareForMpOrSkirmish(IGame game)
    {
        // Move teams from the global list to the skirmish list
        SkirmishTeams.Clear();
        SkirmishTeams.AddRange(Teams);
        Teams.Clear();

        foreach (var skirmishSide in _skirmishSides)
        {
            skirmishSide.Clear();
        }
        _numSkirmishSides = 0;

        // Copy players from _sides to _skirmishSides.
        // Remove players from _sides after the copy, except:
        // - Do not remove the civilian player
        // - Ensure there is always at least one player left in _sides (why?)
        for (var i = 0; i < _numSides; i++)
        {
            _skirmishSides[_numSkirmishSides] = _sides[i];
            _numSkirmishSides++;

            if (_sides[i].Faction == "FactionCivilian")
            {
                continue;
            }

            if (_numSides == 1)
            {
                break;
            }

            RemoveSide(i);
            i--;
        }

        // If any of the players have scripts, don't make any changes
        var gotScripts = Players.Any(p => p.Faction != "FactionCivilian" && p.Scripts?.Scripts.Length > 0);
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

    public Player GetSideInfo(int index)
    {
        return Players[index];
    }

    public Player GetSkirmishSideInfo(int index)
    {
        return SkirmishPlayers[index];
    }

    public (Team Team, int Index)? FindTeamInfo(string teamName)
    {
        var index = Teams.FindIndex(t => t.Name == teamName);
        if (index == -1)
        {
            return null;
        }
        return (Teams[index], index);
    }

    // C++ version returns the index via a pointer parameter, but a nullable tuple is nicer with pattern matching
    public (Player Player, int Index)? FindSideInfo(string playerName)
    {
        var index = Players.FindIndex(p => p.Name == playerName);
        if (index == -1)
        {
            return null;
        }
        return (Players[index], index);
    }

    public (Player Player, int Index)? FindSkirmishSideInfo(string playerName)
    {
        var index = SkirmishPlayers.FindIndex(p => p.Name == playerName);
        if (index == -1)
        {
            return null;
        }
        return (SkirmishPlayers[index], index);
    }

    // Ported from GameLogic::startNewGame
    public bool AddSideAndTeamFromSlot(IGame game, GameSlot slot, GameInfo gameInfo, int i, bool isSkirmishOrSkirmishReplay)
    {
        var settings = game.AssetStore.MultiplayerSettings.Current;

        if (!slot.IsOccupied)
        {
            return false;
        }

        var playerProps = new AssetPropertyCollection();
        var playerName = $"player{i}";
        playerProps.AddAsciiString(PlayerKeys.Name, playerName);

        var playerTemplate = slot.PlayerTemplate switch
        {
            >= 0 => game.AssetStore.PlayerTemplates.GetByIndex((int)slot.PlayerTemplate),
            _ => game.AssetStore.PlayerTemplates.GetByName("FactionObserver")
        };

        if (playerTemplate != null)
        {
            playerProps.AddAsciiString(PlayerKeys.Faction, playerTemplate.Name);
        }

        if (gameInfo.IsPlayerPreorder(i))
        {
            playerProps.AddBoolean(PlayerKeys.IsPreorder, true);
        }

        var otherSlots = gameInfo.Slots.Where(s => s.IsOccupied && s != slot).ToList();
        var allies = otherSlots.Where(slot.IsAlliedTo).Select(s => s.Name);
        var enemies = otherSlots.Where(slot.IsEnemyTo).Select(s => s.Name);
        var alliesString = string.Join(' ', allies);
        var enemiesString = string.Join(' ', enemies);
        playerProps.AddAsciiString(PlayerKeys.Allies, alliesString);
        playerProps.AddAsciiString(PlayerKeys.Enemies, enemiesString);

        playerProps.AddInteger(PlayerKeys.Color, settings.GetColor(slot.Color));
        playerProps.AddInteger(PlayerKeys.NightColor, settings.GetNightColor(slot.Color));
        playerProps.AddInteger(PlayerKeys.MultiplayerStartIndex, slot.StartPos);
        playerProps.AddBoolean(PlayerKeys.MultiplayerIsLocal, slot == gameInfo.LocalSlot);

        if (isSkirmishOrSkirmishReplay)
        {
            playerProps.AddBoolean(PlayerKeys.IsSkirmish, true);
            if (slot.IsAI)
            {
                playerProps.AddInteger(PlayerKeys.SkirmishDifficulty, slot.State switch
                {
                    SlotState.EasyAI => (int)Difficulty.Easy,
                    SlotState.MediumAI => (int)Difficulty.Normal,
                    SlotState.BrutalAI => (int)Difficulty.Hard,
                    _ => throw new InvalidStateException($"Invalid AI difficulty {slot.State}"),
                });
            }
        }

        AddSide(playerProps);

        var teamProps = new AssetPropertyCollection();
        teamProps.AddAsciiString(TeamKeys.Name, $"team{playerName}");
        teamProps.AddAsciiString(TeamKeys.Owner, playerName);
        teamProps.AddBoolean(TeamKeys.IsSingleton, true);

        AddTeam(new Team(teamProps));

        return true;
    }
}

#nullable enable

using System;
using OpenSage.Data.Map;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage;

using Player = Logic.Player;

public class PlayerList : IPersistableObject
{
    private readonly IGame _game;

    public const int MaxPlayerCount = 16;

    private Player? _local;

    // It would be more C#-like to use List<Player>, but we can refactor later if we need to.
    private readonly Player[] _players;
    private int _playerCount;

    public PlayerList(IGame game)
    {
        _game = game;
        _players = new Player[MaxPlayerCount];
        _playerCount = 0;
        _local = null;

        for (var i = 0; i < MaxPlayerCount; i++)
        {
            _players[i] = new Player(new PlayerIndex(i), null, new ColorRgb(0, 0, 0), _game);
        }
    }

    // TODO: Add ISubsystem?
    public void Init(GameInfo? gameInfo)
    {
        _playerCount = 0;

        foreach (var player in _players)
        {
            player.Init(null, gameInfo);
        }

        _local = _players[0];
    }

    public void Reset(GameInfo? gameInfo)
    {
        _game.TeamFactory.Clear();
        Init(gameInfo);
    }

    public void Update()
    {
        foreach (var player in _players)
        {
            player.Update();
        }
    }

    public void NewGame(GameInfo? gameInfo)
    {
        Reset(gameInfo);

        var sidesList = _game.Scene3D.MapFile.SidesList;

        var setLocal = false;

        foreach (var mapPlayer in sidesList.Players)
        {
            var name = mapPlayer.Name ?? string.Empty;
            // If the name is empty, it's the neutral player
            if (name == string.Empty)
            {
                continue;
            }

            var player = _players[_playerCount++];
            player.InitFromDict(mapPlayer, gameInfo);

            if (mapPlayer.Properties.GetPropOrNull(PlayerKeys.MultiplayerIsLocal)?.GetBoolean() ?? false)
            {
                _local = player;
                setLocal = true;
            }

            // TOD(Port): C++ checks here for existance of TheNetwork (meaning it's a multiplayer game?)
            if (!setLocal && (mapPlayer.Properties.GetPropOrNull(PlayerKeys.IsHuman)?.GetBoolean() ?? false))
            {
                _local = player;
                setLocal = true;
            }

            player.BuildList = mapPlayer.BuildList ?? [];
            mapPlayer.BuildList = null;
        }

        if (!setLocal)
        {
            // TODO(Port): Debug crash here as well?
            // No human player found, select first non-neutral player as local player.
            var nonNeutralPlayer = Array.Find(_players, player => player != NeutralPlayer);
            if (nonNeutralPlayer != null)
            {
                nonNeutralPlayer.SetPlayerType(PlayerType.Human, false);
                _local = nonNeutralPlayer;
                setLocal = true;
            }
        }

        _game.TeamFactory.InitFromSides(sidesList);

        // Reset teams

        foreach (var mapPlayer in sidesList.Players)
        {
            var player = FindPlayerWithName(mapPlayer.Name ?? "") ?? throw new InvalidOperationException($"Player not found: {mapPlayer.Name}");
            // TODO: This is from AsciiString.nextToken - should we move this to a more central location?
            char[] separators = [' ', '\n', '\r', '\t'];

            var enemies = (mapPlayer.Enemies ?? "").Split(separators);

            foreach (var enemyName in enemies)
            {
                var enemyPlayer = FindPlayerWithName(enemyName);
                if (enemyPlayer != null)
                {
                    player.SetPlayerRelationship(enemyPlayer, RelationshipType.Enemies);
                }
            }

            var allies = (mapPlayer.Allies ?? "").Split(separators);
            foreach (var allyName in allies)
            {
                var allyPlayer = FindPlayerWithName(allyName);
                if (allyPlayer != null)
                {
                    player.SetPlayerRelationship(allyPlayer, RelationshipType.Allies);
                }
            }

            // Ensure the player is their own ally, and neutral towards the neutral player.
            player.SetPlayerRelationship(player, RelationshipType.Allies);
            if (player != NeutralPlayer)
            {
                player.SetPlayerRelationship(NeutralPlayer, RelationshipType.Neutral);
            }

            player.SetDefaultTeam();
        }
    }

    public void NewMap()
    {
        foreach (var player in _players)
        {
            player.NewMap();
        }
    }

    public void TeamAboutToBeDeleted(Logic.Team team)
    {
        foreach (var player in _players)
        {
            player.RemoveTeamRelationship(team);
        }
    }

    public int PlayerCount => _playerCount;

    public Player? GetNthPlayer(PlayerIndex index)
    {
        if (index.IsInvalid || index.Value >= MaxPlayerCount)
        {
            return null;
        }

        return _players[index.Value];
    }

    public Player NeutralPlayer => _players[0];

    // Should match findPlayerWithNameKey in C++, except for the name key part.
    public Player? FindPlayerWithName(string name)
    {
        return Array.Find(_players, player => player.Name == name);
    }

    public Player? LocalPlayer { get => _local; set => _local = value; }

    public Player? GetPlayerFromMask(PlayerMaskType mask)
    {
        for (var i = 0; i < MaxPlayerCount; i++)
        {
            var player = GetNthPlayer(new PlayerIndex(i));
            if (player?.PlayerMask == mask)
            {
                return player;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first player matching the mask, removes the matching bit from the mask and returns the player.
    /// The name of the method is somewhat misleading, as it doesn't actually get multiple players from the mask;
    /// it's just meant to be called in a loop to get each player.
    /// </summary>
    public Player? GetEachPlayerFromMask(ref PlayerMaskType maskToAdjust)
    {
        for (var i = 0; i < MaxPlayerCount; i++)
        {
            var player = GetNthPlayer(new PlayerIndex(i));
            if (player != null && maskToAdjust.HasFlag(player.PlayerMask))
            {
                maskToAdjust &= ~player.PlayerMask;
                return player;
            }
        }

        // TODO(Port): Debug crash if we reach here?
        maskToAdjust = PlayerMaskType.None;
        return null;
    }

    public Logic.Team ValidateTeam(string owner)
    {
        var team = _game.TeamFactory.FindTeam(owner);

        if (team == null)
        {
            // TODO: Generals has debug assert here and a fallback to neutral player's default team.
            // What should we do?
            throw new InvalidOperationException($"Team not found for owner: {owner}");
        }

        return team;
    }

    public void UpdateTeamStates()
    {
        foreach (var player in _players)
        {
            player.UpdateTeamStates();
        }
    }

    PlayerMaskType GetPlayersWithRelationship(PlayerIndex srcPlayerIndex, AllowPlayerRelationship allowedRelationships)
    {
        var mask = PlayerMaskType.None;

        if (allowedRelationships == 0)
        {
            return mask;
        }

        var srcPlayer = GetNthPlayer(srcPlayerIndex);

        if (srcPlayer == null)
        {
            return mask;
        }

        if (allowedRelationships.HasFlag(AllowPlayerRelationship.SamePlayer))
        {
            mask |= srcPlayer.PlayerMask;
        }

        for (var i = 0; i < _playerCount; i++)
        {
            var player = GetNthPlayer(new PlayerIndex(i));

            if (player == null || player == srcPlayer)
            {
                continue;
            }

            var relationship = srcPlayer.GetRelationship(player.DefaultTeam);

            switch (relationship)
            {
                case RelationshipType.Enemies:
                    if (allowedRelationships.HasFlag(AllowPlayerRelationship.Enemies))
                    {
                        mask |= player.PlayerMask;
                    }
                    break;
                case RelationshipType.Allies:
                    if (allowedRelationships.HasFlag(AllowPlayerRelationship.Allies))
                    {
                        mask |= player.PlayerMask;
                    }
                    break;
                case RelationshipType.Neutral:
                    if (allowedRelationships.HasFlag(AllowPlayerRelationship.Neutrals))
                    {
                        mask |= player.PlayerMask;
                    }
                    break;
            }
        }

        return mask;
    }

    public void Persist(StatePersister persister)
    {
        persister.PersistVersion(1);

        var playerCount = _playerCount;
        persister.PersistInt32(ref playerCount);

        if (playerCount != _playerCount)
        {
            throw new InvalidStateException($"Player count mismatch: {playerCount} != {_playerCount}");
        }

        for (var i = 0; i < _playerCount; i++)
        {
            var player = _players[i];
            player.Persist(persister);
        }
    }
}

public enum AllowPlayerRelationship : ushort
{
    SamePlayer = 0x01,
    Allies = 0x02,
    Enemies = 0x04,
    Neutrals = 0x08,
}

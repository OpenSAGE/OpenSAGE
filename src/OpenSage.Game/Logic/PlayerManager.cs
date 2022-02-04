using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic
{
    public sealed class PlayerManager : IPersistableObject
    {
        private readonly Game _game;

        public IReadOnlyList<Player> Players => _players;
        private Player[] _players;

        public Player LocalPlayer { get; private set; }

        internal PlayerManager(Game game)
        {
            _game = game;
            _players = Array.Empty<Player>();
        }

        internal void OnNewGame(Data.Map.Player[] mapPlayers, GameType gameType)
        {
            _players = CreatePlayers(mapPlayers, gameType).ToArray();

            LocalPlayer = null;

            foreach (var player in _players)
            {
                if (player.IsHuman)
                {
                    LocalPlayer = player;
                    break;
                }
            }

            if (LocalPlayer == null && _players.Length > 2)
            {
                // TODO: Probably not the right way to do it.
                LocalPlayer = _players[2];
            }

            // TODO: Setup player relationships.
        }

        // This needs to operate on the entire player list, because players have references to each other
        // (allies and enemies).
        private IEnumerable<Player> CreatePlayers(Data.Map.Player[] mapPlayers, GameType gameType)
        {
            var players = new Dictionary<string, Player>();
            var allies = new Dictionary<string, string[]>();
            var enemies = new Dictionary<string, string[]>();

            var id = 0u;
            foreach (var mapPlayer in mapPlayers)
            {
                var player = Player.FromMapData(id++, mapPlayer, _game, gameType != GameType.SinglePlayer);
                players[player.Name] = player;
                allies[player.Name] =
                    (mapPlayer.Properties.GetPropOrNull("playerAllies")?.Value as string)?.Split(' ');
                enemies[player.Name] =
                    (mapPlayer.Properties.GetPropOrNull("playerEnemies")?.Value as string)?.Split(' ');
            }

            foreach (var (name, player) in players)
            {
                player.Allies = allies[name].Select(ally => players[ally]).ToSet();
                player.Enemies = enemies[name].Select(enemy => players[enemy]).ToSet();
            }

            return players.Values;
        }

        public Player GetPlayerByName(string name)
        {
            return Array.Find(_players, x => x.Name == name);
        }

        public Player GetPlayerByIndex(uint index)
        {
            return _players[(int)index];
        }

        public int GetPlayerIndex(Player player)
        {
            return Array.IndexOf(_players, player);
        }

        // TODO: Is this right?
        public Player GetCivilianPlayer() => _players[1];

        internal void LogicTick()
        {
            foreach (var player in _players)
            {
                player.LogicTick();
            }
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistArrayWithUInt32Length(
                _players,
                static (StatePersister persister, ref Player item) =>
                {
                    persister.PersistObjectValue(item);
                });
        }
    }
}

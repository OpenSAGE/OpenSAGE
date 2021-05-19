using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Map;
using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    public sealed class PlayerManager
    {
        public IReadOnlyList<Player> Players => _players;
        private List<Player> _players;

        public Player LocalPlayer { get; private set; }

        internal PlayerManager()
        {
            _players = new List<Player>();
        }

        internal void OnNewGame(MapFile mapFile, Game game)
        {
            _players = Player.FromMapData(mapFile.SidesList.Players, game.AssetStore).ToList();

            // TODO: This is completely wrong.
            LocalPlayer = _players[2];
        }

        internal void SetSkirmishPlayers(IEnumerable<Player> players, Player localPlayer)
        {
            _players = players.ToList();

            if (!_players.Contains(localPlayer))
            {
                throw new ArgumentException(
                    $"Argument {nameof(localPlayer)} should be included in {nameof(players)}",
                    nameof(localPlayer));
            }

            LocalPlayer = localPlayer;
        }

        internal void AddReplayObserver(Game game)
        {
            _players.Add(
                new Player(
                    game.AssetStore.PlayerTemplates.GetByName("FactionObserver"),
                    new Mathematics.ColorRgb(),
                    game.AssetStore)
                {
                    Name = "ReplayObserver"
                });
        }

        public Player GetPlayerByName(string name)
        {
            return _players.Find(x => x.Name == name);
        }

        public int GetPlayerIndex(Player player)
        {
            return _players.IndexOf(player);
        }

        internal void LogicTick()
        {
            foreach (var player in _players)
            {
                player.LogicTick();
            }
        }

        internal void Load(SaveFileReader reader, Game game)
        {
            reader.ReadVersion(1);

            var numPlayers = reader.ReadUInt32();
            if (numPlayers != _players.Count)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < numPlayers; i++)
            {
                _players[i].Load(reader, game);
            }
        }
    }
}

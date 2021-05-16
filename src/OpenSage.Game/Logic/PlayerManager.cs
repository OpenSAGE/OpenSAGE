using System;
using System.Collections.Generic;
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
            LocalPlayer = _players.FirstOrDefault();
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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var numPlayers = reader.ReadUInt32();

            //var players = new Logic.Player[numPlayers];
            //for (var i = 0; i < numPlayers; i++)
            //{
            //    players[i] = new Logic.Player(null, new ColorRgb(), game.AssetStore.Ranks);
            //    players[i].Load(reader, game.AssetStore);
            //}
        }
    }
}

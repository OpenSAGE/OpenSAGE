using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Content;
using OpenSage.Data.Sav;
using OpenSage.Utilities.Extensions;

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

        internal void OnNewGame(Data.Map.Player[] mapPlayers, Game game, GameType gameType)
        {
            _players = CreatePlayers(mapPlayers, game.AssetStore, gameType).ToList();

            foreach (var player in _players)
            {
                if (player.IsHuman)
                {
                    LocalPlayer = player;
                    break;
                }
            }

            // TODO: Setup player relationships.
        }

        // This needs to operate on the entire player list, because players have references to each other
        // (allies and enemies).
        private static IEnumerable<Player> CreatePlayers(Data.Map.Player[] mapPlayers, AssetStore assetStore, GameType gameType)
        {
            var players = new Dictionary<string, Player>();
            var allies = new Dictionary<string, string[]>();
            var enemies = new Dictionary<string, string[]>();

            var id = 0u;
            foreach (var mapPlayer in mapPlayers)
            {
                var player = Player.FromMapData(id++, mapPlayer, assetStore, gameType != GameType.SinglePlayer);
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
            return _players.Find(x => x.Name == name);
        }

        public Player GetPlayerByIndex(uint index)
        {
            return _players[(int)index];
        }

        public int GetPlayerIndex(Player player)
        {
            return _players.IndexOf(player);
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

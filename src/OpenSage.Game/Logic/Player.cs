using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic
{
    [DebuggerDisplay("[Player: {Name}]")]
    public class Player
    {
        public PlayerTemplate Template { get; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }

        public string Side { get; private set; }

        public bool IsHuman { get; private set; }

        public uint Money { get; set; }

        // TODO: Should this be derived from the player's buildings so that it doesn't get out of sync?
        public uint Energy { get; set; }

        public ColorRgb Color { get; }

        private HashSet<Player> _allies;
        public IReadOnlyCollection<Player> Allies => _allies;

        private HashSet<Player> _enemies;
        public IReadOnlyCollection<Player> Enemies => _enemies;

        // TODO: Does the order matter? Is it ever visible in UI?
        // TODO: Yes the order does matter. For example, the sound played when moving mixed groups of units is the one for the most-recently-selected unit.
        private HashSet<GameObject> _selectedUnits;
        public IReadOnlyCollection<GameObject> SelectedUnits => _selectedUnits;

        public GameObject HoveredUnit { get; set; }

        public Player(PlayerTemplate template, in ColorRgb color)
        {
            Template = template;
            Color = color;
            _selectedUnits = new HashSet<GameObject>();
            _allies = new HashSet<Player>();
            _enemies = new HashSet<Player>();
        }

        public void SelectUnits(IEnumerable<GameObject> units, bool additive = false)
        {
            if (additive)
            {
                _selectedUnits.UnionWith(units);
            }
            else
            {
                _selectedUnits = units.ToSet();
            }
            foreach (var unit in _selectedUnits)
            {
                unit.IsSelected = true;
            }
        }

        public void DeselectUnits()
        {
            foreach (var unit in _selectedUnits)
            {
                unit.IsSelected = false;
            }
            _selectedUnits.Clear();
        }

        public bool CanProduceObject(GameObjectCollection allGameObjects, ObjectDefinition objectToProduce)
        {
            if (objectToProduce.Prerequisites == null)
            {
                return true;
            }

            // TODO: Make this more efficient.
            bool HasPrerequisite(ObjectDefinition prerequisite)
            {
                foreach (var gameObject in allGameObjects.Items)
                {
                    if (gameObject.Owner == this && gameObject.Definition == prerequisite)
                    {
                        return true;
                    }
                }

                return false;
            }

            // Prerequisites are AND'd.
            foreach (var prerequisiteList in objectToProduce.Prerequisites.Objects)
            {
                // The list within each prerequisite is OR'd.

                var hasPrerequisite = false;
                foreach (var prerequisite in prerequisiteList)
                {
                    if (HasPrerequisite(prerequisite.Value))
                    {
                        hasPrerequisite = true;
                        break;
                    }
                }

                if (!hasPrerequisite)
                {
                    return false;
                }
            }

            return true;
        }

        private static Player FromMapData(Data.Map.Player mapPlayer, AssetStore assetStore)
        {
            var side = mapPlayer.Properties["playerFaction"].Value as string;

            // We need the template for default values
            var template = assetStore.PlayerTemplates.GetByName(side);

            var name = mapPlayer.Properties["playerName"].Value as string;
            var displayName = mapPlayer.Properties["playerDisplayName"].Value as string;
            var translatedDisplayName = displayName.Translate();

            var isHuman = (bool) mapPlayer.Properties["playerIsHuman"].Value;

            var colorRgb = mapPlayer.Properties.GetPropOrNull("playerColor")?.Value as uint?;

            ColorRgb color;

            if (colorRgb != null)
            {
                color = ColorRgb.FromUInt32(colorRgb.Value);
            }
            else if (template != null) // Template is null for the neutral faction
            {
                color = template.PreferredColor;
            }
            else
            {
                color = new ColorRgb(0, 0, 0);
            }

            return new Player(template, color)
            {
                Side = side,
                Name = name,
                DisplayName = translatedDisplayName,
                IsHuman = isHuman
            };
        }

        // This needs to operate on the entire player list, because players have references to each other
        // (allies and enemies).
        internal static IEnumerable<Player> FromMapData(Data.Map.Player[] mapPlayers, AssetStore assetStore)
        {
            var players = new Dictionary<string, Player>();
            var allies = new Dictionary<string, string[]>();
            var enemies = new Dictionary<string, string[]>();

            foreach (var mapPlayer in mapPlayers)
            {
                var player = FromMapData(mapPlayer, assetStore);
                players[player.Name] = player;
                allies[player.Name] =
                    (mapPlayer.Properties.GetPropOrNull("playerAllies")?.Value as string)?.Split(' ');
                enemies[player.Name] =
                    (mapPlayer.Properties.GetPropOrNull("playerEnemies")?.Value as string)?.Split(' ');
            }

            foreach (var (name, player) in players)
            {
                player._allies = allies[name].Select(ally => players[ally]).ToSet();
                player._enemies = enemies[name].Select(enemy => players[enemy]).ToSet();
            }

            return players.Values;
        }

        public static Player FromTemplate(GameData gameData, PlayerTemplate template, PlayerSetting? setting = null)
        {
            var color = setting.HasValue ? setting.Value.Color : template.PreferredColor;

            // TODO: Use rest of the properties from the template
            return new Player(template, color)
            {
                Side = template.Side,
                Name = setting == null ? template.Name : setting?.Name,
                DisplayName = template.DisplayName.Translate(),
                Money = (uint)(template.StartMoney + gameData.DefaultStartingCash),
                IsHuman = setting?.Owner == PlayerOwner.Player
            };
        }
    }

    public class Team
    {
        public string Name { get; }
        public Player Owner { get; }
        public bool IsSingleton { get; }

        public Team(string name, Player owner, bool isSingleton)
        {
            Name = name;
            Owner = owner;
            IsSingleton = isSingleton;
        }

        public static Team FromMapData(Data.Map.Team mapTeam, IList<Player> players)
        {
            var name = mapTeam.Properties["teamName"].Value as string;

            var ownerName = mapTeam.Properties["teamOwner"].Value as string;
            var owner = players.FirstOrDefault(player => player.Name == ownerName);

            var isSingleton = (bool) mapTeam.Properties["teamIsSingleton"].Value;

            return new Team(name, owner, isSingleton);
        }
    }
}

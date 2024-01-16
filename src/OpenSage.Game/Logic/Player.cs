using System.Collections.Generic;
using System.Diagnostics;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Data.Map;
using OpenSage.Logic.AI;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic
{
    [DebuggerDisplay("[Player: {Name}]")]
    public class Player : IPersistableObject
    {
        public const int MaxPlayers = 16;

        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Game _game;

        public readonly SupplyManager SupplyManager;

        private readonly List<Upgrade> _upgrades;
        private readonly UpgradeSet _upgradesInProgress;

        public readonly UpgradeSet UpgradesCompleted;

        private readonly ScienceSet _sciences;
        private readonly ScienceSet _sciencesDisabled;
        private readonly ScienceSet _sciencesHidden;

        private readonly PlayerRelationships _playerToPlayerRelationships = new PlayerRelationships();
        private readonly PlayerRelationships _playerToTeamRelationships = new PlayerRelationships();

        private readonly List<TeamTemplate> _teamTemplates;

        private uint _unknown1;
        private bool _unknown2;
        private uint _unknown3;
        private bool _hasInsufficientPower;
        private readonly List<BuildListItem> _buildListItems = new();
        private TunnelManager _tunnelManager;
        private uint _unknown4;
        private uint _unknown5;
        private bool _unknown6;
        private readonly bool[] _attackedByPlayerIds = new bool[MaxPlayers];
        private readonly PlayerScoreManager _scoreManager = new();
        private readonly List<ObjectIdSet> _controlGroups = new();
        private readonly ObjectIdSet _destroyedObjects = new();

        public uint Id { get; }
        public PlayerTemplate Template { get; }
        public string Name;
        public string DisplayName { get; private set; }

        public string Side { get; private set; }

        public bool IsHuman { get; private set; }

        public Team DefaultTeam { get; private set; }

        public readonly BankAccount BankAccount;

        public Rank Rank { get; set; }
        public uint SkillPointsTotal;
        public uint SkillPointsAvailable;
        public uint SciencePurchasePoints { get; set; }
        public bool CanBuildUnits;
        public bool CanBuildBuildings;
        public float GeneralsExperienceMultiplier;
        public bool ShowOnScoreScreen;

        public AIPlayer AIPlayer { get; private set; }

        // TODO: Should this be derived from the player's buildings so that it doesn't get out of sync?
        public int GetEnergy(GameObjectCollection allGameObjects)
        {
            var energy = 0;
            foreach (var gameObject in allGameObjects.Items)
            {
                if (gameObject.Owner != this)
                {
                    continue;
                }
                energy += gameObject.EnergyProduction;
            }
            return energy;
        }

        public void LogicTick()
        {
            Rank.Update();
        }

        public bool SpecialPowerAvailable(SpecialPower specialPower)
        {
            if (specialPower.RequiredSciences != null)
            {
                foreach (var requirement in specialPower.RequiredSciences)
                {
                    if (!HasScience(requirement.Value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public ColorRgb Color { get; }

        public HashSet<Player> Allies { get; internal set; }

        public HashSet<Player> Enemies { get; internal set; }

        private HashSet<GameObject>[] _selectionGroups;

        // TODO: Does the order matter? Is it ever visible in UI?
        // TODO: Yes the order does matter. For example, the sound played when moving mixed groups of units is the one for the most-recently-selected unit.
        private HashSet<GameObject> _selectedUnits;
        public IReadOnlyCollection<GameObject> SelectedUnits => _selectedUnits;

        public GameObject HoveredUnit { get; set; }

        public int Team { get; init; }

        public Player(uint id, PlayerTemplate template, in ColorRgb color, Game game)
        {
            _game = game;

            Id = id;
            Template = template;
            Color = color;
            _selectionGroups = new HashSet<GameObject>[10];
            _selectedUnits = new HashSet<GameObject>();
            Allies = new HashSet<Player>();
            Enemies = new HashSet<Player>();

            SupplyManager = new SupplyManager(game);

            _upgrades = new List<Upgrade>();
            _upgradesInProgress = new UpgradeSet();
            UpgradesCompleted = new UpgradeSet();

            _sciences = new ScienceSet();
            _sciencesDisabled = new ScienceSet();
            _sciencesHidden = new ScienceSet();

            _teamTemplates = new List<TeamTemplate>();

            Rank = new Rank(this, game.AssetStore.Ranks);

            if (template?.InitialUpgrades != null)
            {
                foreach (var upgrade in template.InitialUpgrades)
                {
                    AddUpgrade(upgrade.Value, UpgradeStatus.Completed);
                }
            }

            if (template?.IntrinsicSciences != null)
            {
                foreach (var science in template.IntrinsicSciences)
                {
                    _sciences.Add(science.Value);
                }
            }

            BankAccount = new BankAccount();
        }

        internal void SelectUnits(IEnumerable<GameObject> units, bool additive = false)
        {
            if (additive)
            {
                _selectedUnits.UnionWith(units);
            }
            else
            {
                _selectedUnits = units.ToSet();
            }

            var unitsFromHordeSelection = new List<GameObject>();
            foreach (var unit in _selectedUnits)
            {
                unit.IsSelected = true;

                if (unit.ParentHorde != null && !unit.ParentHorde.IsSelected)
                {
                    unitsFromHordeSelection.Add(unit.ParentHorde);
                    unitsFromHordeSelection.AddRange(unit.ParentHorde.FindBehavior<HordeContainBehavior>()?.SelectAll(true));
                }
                else
                {
                    var hordeContain = unit.FindBehavior<HordeContainBehavior>();
                    if (hordeContain != null)
                    {
                        unitsFromHordeSelection.AddRange(hordeContain.SelectAll(true));
                    }
                }
            }
            _selectedUnits.UnionWith(unitsFromHordeSelection);
        }

        public void DeselectUnits()
        {
            foreach (var unit in _selectedUnits)
            {
                unit.IsSelected = false;

                if (unit.ParentHorde != null && unit.ParentHorde.IsSelected)
                {
                    unit.ParentHorde.FindBehavior<HordeContainBehavior>()?.SelectAll(false);
                }
                else
                {
                    var hordeContain = unit.FindBehavior<HordeContainBehavior>();
                    if (hordeContain != null)
                    {
                        hordeContain.SelectAll(false);
                    }
                }
            }
            _selectedUnits.Clear();
        }

        public void CreateSelectionGroup(int idx)
        {
            if(idx > _selectionGroups.Length)
            {
                Logger.Warn($"Do not support more than { _selectionGroups.Length} groups!");
                return;
            }

            // TODO: when one game object dies we need to remove it from these groups
            _selectionGroups[idx] = new HashSet<GameObject>(_selectedUnits);
        }

        public void SelectGroup(int idx)
        {
            if (idx > _selectionGroups.Length)
            {
                Logger.Warn($"Do not support more than { _selectionGroups.Length} groups!");
                return;
            }

            // TODO: when one game object dies we need to remove it from these groups
            SelectUnits(_selectionGroups[idx]);
        }

        public bool ScienceAvailable(Science science)
        {
            if (HasScience(science))
            {
                return false;
            }

            if (_sciencesDisabled.Contains(science))
            {
                return false;
            }

            if (_sciencesHidden.Contains(science))
            {
                return false;
            }

            foreach (var requiredScience in science.PrerequisiteSciences)
            {
                if (requiredScience.Value == null)
                {
                    continue;
                }

                if (!_sciences.Contains(requiredScience.Value))
                {
                    return false;
                }
            }

            return science.SciencePurchasePointCost <= SciencePurchasePoints;
        }

        public void PurchaseScience(Science science)
        {
            if (!ScienceAvailable(science))
            {
                Logger.Warn("Trying to purchase science without fullfilling requirements");
                return;
            }

            if (!science.IsGrantable)
            {
                return;
            }

            SciencePurchasePoints -= (uint) science.SciencePurchasePointCost;
            _sciences.Add(science);
        }

        public bool HasScience(Science science)
        {
            return _sciences.Contains(science);
        }

        public bool CanProduceObject(ObjectDefinition objectToProduce)
        {
            if (objectToProduce.Prerequisites == null)
            {
                return true;
            }

            // TODO: Make this more efficient.
            bool HasPrerequisite(ObjectDefinition prerequisite)
            {
                foreach (var gameObject in _game.GameLogic.Objects)
                {
                    if (gameObject.Owner == this && gameObject.Definition == prerequisite && gameObject.BuildProgress >= 1.0)
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

        internal Upgrade AddUpgrade(UpgradeTemplate template, UpgradeStatus status)
        {
            Upgrade upgrade = null;
            foreach (var eachUpgrade in _upgrades)
            {
                if (eachUpgrade.Template == template)
                {
                    upgrade = eachUpgrade;
                    break;
                }
            }

            if (upgrade == null)
            {
                upgrade = new Upgrade(template);
            }

            upgrade.Status = status;

            _upgrades.Add(upgrade);

            switch (status)
            {
                case UpgradeStatus.Queued:
                    _upgradesInProgress.Add(template);
                    break;

                case UpgradeStatus.Completed:
                    _upgradesInProgress.Remove(template);
                    UpgradesCompleted.Add(template);
                    // TODO: Iterate all game objects owned by this player and call their UpdateUpgradeableModules methods.
                    break;
            }

            return upgrade;
        }

        internal void RemoveUpgrade(UpgradeTemplate template)
        {
            Upgrade upgradeToRemove = null;

            foreach (var upgrade in _upgrades)
            {
                if (upgrade.Template == template)
                {
                    upgradeToRemove = upgrade;
                    break;
                }
            }

            if (upgradeToRemove != null)
            {
                _upgrades.Remove(upgradeToRemove);
            }
        }

        public bool HasUpgrade(UpgradeTemplate template)
        {
            foreach (var upgrade in _upgrades)
            {
                if (upgrade.Template == template)
                {
                    return true;
                }
            }

            return false;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(8);

            reader.PersistObject(BankAccount);

            var upgradeQueueCount = (ushort)_upgrades.Count;
            reader.PersistUInt16(ref upgradeQueueCount);

            reader.SkipUnknownBytes(1);

            reader.PersistObject(_sciencesDisabled);
            reader.PersistObject(_sciencesHidden);

            reader.BeginArray("Upgrades");
            if (reader.Mode == StatePersistMode.Read)
            {
                for (var i = 0; i < upgradeQueueCount; i++)
                {
                    reader.BeginObject();

                    var upgradeName = "";
                    reader.PersistAsciiString(ref upgradeName);
                    var upgradeTemplate = reader.AssetStore.Upgrades.GetByName(upgradeName);

                    // Use UpgradeStatus.Invalid temporarily because we're going to load the
                    // actual queued / completed status below.
                    var upgrade = AddUpgrade(upgradeTemplate, UpgradeStatus.Invalid);

                    reader.PersistObject(upgrade);

                    reader.EndObject();
                }
            }
            else
            {
                foreach (var upgrade in _upgrades)
                {
                    reader.BeginObject();

                    var upgradeName = upgrade.Template.Name;
                    reader.PersistAsciiString(ref upgradeName);

                    reader.PersistObject(upgrade);

                    reader.EndObject();
                }
            }
            reader.EndArray();

            reader.PersistUInt32(ref _unknown1);
            reader.PersistBoolean(ref _unknown2);
            reader.PersistUInt32(ref _unknown3);
            reader.PersistBoolean(ref _hasInsufficientPower);
            reader.PersistObject(_upgradesInProgress);
            reader.PersistObject(UpgradesCompleted);

            {
                reader.BeginObject("UnknownThing");

                reader.PersistVersion(2);

                var playerId = Id;
                reader.PersistUInt32(ref playerId);
                if (playerId != Id)
                {
                    throw new InvalidStateException();
                }

                reader.EndObject();
            }

            reader.PersistList(
                _teamTemplates,
                static (StatePersister persister, ref TeamTemplate item) =>
                {
                    var teamTemplateId = item?.ID ?? 0u;
                    persister.PersistUInt32Value(ref teamTemplateId);

                    if (persister.Mode == StatePersistMode.Read)
                    {
                        item = persister.Game.TeamFactory.FindTeamTemplateById(teamTemplateId);
                    }
                });

            if (reader.Mode == StatePersistMode.Read)
            {
                foreach (var teamTemplate in _teamTemplates)
                {
                    if (teamTemplate.Owner != this)
                    {
                        throw new InvalidStateException();
                    }
                }
            }

            reader.PersistList(
                _buildListItems,
                static (StatePersister persister, ref BuildListItem item) =>
                {
                    item ??= new BuildListItem();
                    persister.PersistObjectValue(item);
                });

            var isAIPlayer = AIPlayer != null;
            reader.PersistBoolean(ref isAIPlayer);
            if (isAIPlayer != (AIPlayer != null))
            {
                throw new InvalidStateException();
            }
            if (isAIPlayer)
            {
                reader.PersistObject(AIPlayer);
            }

            var hasSupplyManager = SupplyManager != null;
            reader.PersistBoolean(ref hasSupplyManager);
            if (hasSupplyManager)
            {
                reader.PersistObject(SupplyManager);
            }

            var hasTunnelManager = _tunnelManager != null;
            reader.PersistBoolean(ref hasTunnelManager);
            if (hasTunnelManager)
            {
                _tunnelManager ??= new TunnelManager();
                reader.PersistObject(_tunnelManager);
            }

            var defaultTeamId = DefaultTeam?.Id ?? 0u;
            reader.PersistUInt32(ref defaultTeamId);
            if (reader.Mode == StatePersistMode.Read)
            {
                DefaultTeam = reader.Game.TeamFactory.FindTeamById(defaultTeamId);
                if (DefaultTeam.Template.Owner != this)
                {
                    throw new InvalidStateException();
                }
            }

            reader.PersistObject(_sciences);

            var rankId = (uint)Rank.CurrentRank;
            reader.PersistUInt32(ref rankId);
            Rank.SetRank((int) rankId);

            reader.PersistUInt32(ref SkillPointsTotal);
            reader.PersistUInt32(ref SkillPointsAvailable);
            reader.PersistUInt32(ref _unknown4); // 800
            reader.PersistUInt32(ref _unknown5); // 0
            reader.PersistUnicodeString(ref Name);
            reader.PersistObject(_playerToPlayerRelationships);
            reader.PersistObject(_playerToTeamRelationships);
            reader.PersistBoolean(ref CanBuildUnits);
            reader.PersistBoolean(ref CanBuildBuildings);
            reader.PersistBoolean(ref _unknown6);
            reader.PersistSingle(ref GeneralsExperienceMultiplier);
            reader.PersistBoolean(ref ShowOnScoreScreen);

            reader.PersistArray(
                _attackedByPlayerIds,
                static (StatePersister persister, ref bool item) =>
                {
                    persister.PersistBooleanValue(ref item);
                });

            reader.SkipUnknownBytes(70);

            reader.PersistObject(_scoreManager);

            reader.SkipUnknownBytes(4);

            reader.PersistList(
                _controlGroups, static (StatePersister persister, ref ObjectIdSet item) =>
                {
                    item ??= new ObjectIdSet();
                    persister.PersistObjectValue(item);
                });

            var unknown = true;
            reader.PersistBoolean(ref unknown);
            if (!unknown)
            {
                throw new InvalidStateException();
            }

            reader.PersistObject(_destroyedObjects);

            reader.SkipUnknownBytes(14);
        }

        public static Player FromMapData(uint index, Data.Map.Player mapPlayer, Game game, bool isSkirmish)
        {
            var side = mapPlayer.Properties["playerFaction"].Value as string;

            if (side.StartsWith("FactionAmerica", System.StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: Probably not right.
                side = "FactionAmerica";
            }
            else if (side.StartsWith("FactionChina", System.StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: Probably not right.
                side = "FactionChina";
            }
            else if (side.StartsWith("FactionGLA", System.StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: Probably not right.
                side = "FactionGLA";
            }

            // We need the template for default values
            var template = game.AssetStore.PlayerTemplates.GetByName(side);

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

            var result = new Player(index, template, color, game)
            {
                Side = side,
                Name = name,
                DisplayName = translatedDisplayName,
                IsHuman = isHuman,
            };

            result.AIPlayer = isHuman || template == null || side == "FactionObserver"
                ? null
                : isSkirmish && side != "FactionCivilian"
                    ? new SkirmishAIPlayer(result)
                    : new AIPlayer(result);

            if (template != null)
            {
                result.BankAccount.Money = (uint) (template.StartMoney + game.AssetStore.GameData.Current.DefaultStartingCash);
            }

            return result;
        }

        public void AddAlly(Player player)
        {
            Allies.Add(player);
        }

        public void AddEnemy(Player player)
        {
            Enemies.Add(player);
        }
    }

    public sealed class SupplyManager : IPersistableObject
    {
        private readonly Game _game;

        private readonly ObjectIdSet _supplyWarehouses;
        private readonly ObjectIdSet _supplyCenters;

        internal SupplyManager(Game game)
        {
            _game = game;

            _supplyWarehouses = new ObjectIdSet();
            _supplyCenters = new ObjectIdSet();
        }

        public void AddSupplyCenter(GameObject supplyCenter)
        {
            _supplyCenters.Add(supplyCenter.ID);
        }

        public void RemoveSupplyCenter(GameObject supplyCenter)
        {
            _supplyCenters.Remove(supplyCenter.ID);
        }

        public void AddSupplyWarehouse(GameObject supplyWarehouse)
        {
            _supplyWarehouses.Add(supplyWarehouse.ID);
        }

        public void RemoveSupplyWarehouse(GameObject supplyWarehouse)
        {
            _supplyWarehouses.Remove(supplyWarehouse.ID);
        }

        public GameObject FindClosestSupplyCenter(GameObject supplyGatherer)
        {
            GameObject closestSupplyCenter = null;
            var closestDistanceSquared = float.MaxValue;

            foreach (var supplyCenterId in _supplyCenters)
            {
                var supplyCenter = _game.GameLogic.GetObjectById(supplyCenterId);

                if (supplyCenter.Owner != supplyGatherer.Owner)
                {
                    continue;
                }

                var dockUpdate = supplyCenter.FindBehavior<SupplyCenterDockUpdate>();
                if (!dockUpdate?.CanApproach() ?? false)
                {
                    continue;
                }

                var distanceSquared = _game.PartitionCellManager.GetDistanceBetweenObjectsSquared(supplyGatherer, supplyCenter);

                if (distanceSquared < closestDistanceSquared)
                {
                    closestSupplyCenter = supplyCenter;
                    closestDistanceSquared = distanceSquared;
                }
            }

            return closestSupplyCenter;
        }

        public GameObject FindClosestSupplyWarehouse(GameObject supplyGatherer)
        {
            GameObject closestSupplyWarehouse = null;
            var closestDistanceSquared = float.MaxValue;

            var supplyAIUpdate = supplyGatherer.FindBehavior<SupplyAIUpdate>();
            if (supplyAIUpdate != null)
            {
                closestDistanceSquared = supplyAIUpdate.SupplyWarehouseScanDistance * supplyAIUpdate.SupplyWarehouseScanDistance;
            }

            foreach (var supplyWarehouseId in _supplyWarehouses)
            {
                var supplyWarehouse = _game.GameLogic.GetObjectById(supplyWarehouseId);

                var dockUpdate = supplyWarehouse.FindBehavior<SupplyWarehouseDockUpdate>();

                if (!dockUpdate?.CanApproach() ?? false)
                {
                    continue;
                }

                if (!dockUpdate?.HasBoxes() ?? false)
                {
                    continue;
                }

                var distanceSquared = _game.PartitionCellManager.GetDistanceBetweenObjectsSquared(supplyGatherer, supplyWarehouse);

                if (distanceSquared < closestDistanceSquared)
                {
                    closestSupplyWarehouse = supplyWarehouse;
                    closestDistanceSquared = distanceSquared;
                }
            }

            return closestSupplyWarehouse;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObject(_supplyWarehouses);
            reader.PersistObject(_supplyCenters);
        }
    }

    public enum RelationshipType : uint
    {
        Enemies = 0,
        Neutral = 1,
        Allies = 2,
    }

    public sealed class PlayerRelationships : IPersistableObject
    {
        private readonly Dictionary<uint, RelationshipType> _store = new();

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistDictionary(
                _store,
                static (StatePersister persister, ref uint key, ref RelationshipType value) =>
            {
                persister.PersistUInt32(ref key, "PlayerOrTeamId");
                persister.PersistEnum(ref value, "RelationshipType");
            },
            "Dictionary");
        }
    }

    public sealed class ScienceSet : HashSet<Science>, IPersistableObject
    {
        public void Persist(StatePersister persister)
        {
            persister.PersistVersion(1);

            persister.PersistHashSet(
                this,
                static (StatePersister persister, ref Science item) =>
                {
                    var name = item?.Name ?? "";
                    persister.PersistAsciiStringValue(ref name);

                    if (persister.Mode == StatePersistMode.Read)
                    {
                        item = persister.AssetStore.Sciences.GetByName(name);
                    }
                },
                "Values");
        }
    }

    public sealed class UpgradeSet : HashSet<UpgradeTemplate>, IPersistableObject
    {
        public void Persist(StatePersister persister)
        {
            persister.PersistVersion(1);

            persister.PersistHashSet(
                this,
                static (StatePersister persister, ref UpgradeTemplate item) =>
                {
                    var name = item?.Name ?? "";
                    persister.PersistAsciiStringValue(ref name);

                    if (persister.Mode == StatePersistMode.Read)
                    {
                        item = persister.AssetStore.Upgrades.GetByName(name);
                    }
                },
                "Values");
        }
    }

    public sealed class ObjectIdSet : HashSet<uint>, IPersistableObject
    {
        public void Persist(StatePersister persister)
        {
            persister.PersistVersion(1);

            persister.PersistHashSet(
                this,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistUInt32Value(ref item);
                },
                "Values");
        }
    }

    internal sealed class PlayerStatObjectCollection : Dictionary<string, uint>, IPersistableObject
    {
        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistDictionary(
                this,
                static (StatePersister persister, ref string key, ref uint value) =>
                {
                    persister.PersistAsciiString(ref key, "ObjectType");
                    persister.PersistUInt32(ref value, "Total");
                },
                "Dictionary");
        }
    }

    public enum UpgradeStatus
    {
        Invalid = 0,
        Queued = 1,
        Completed = 2
    }

    public sealed class BankAccount : IPersistableObject
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public uint Money;

        public void Withdraw(uint amount)
        {
            // TODO: Play MoneyWithdrawSound

            if (Money >= amount)
            {
                Money -= amount;
            }
            else
            {
                // this should not happen since we should check first if we can spend that much
                Logger.Warn($"Spent more money ({amount}) than player had ({Money})!");
                Money = 0;
            }
        }

        public void Deposit(uint amount)
        {
            // TODO: Play MoneyDepositSound

            Money += amount;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref Money);
        }
    }

    public sealed class TunnelManager : IPersistableObject
    {
        private readonly ObjectIdSet _tunnelIds = new();
        private readonly List<uint> _containedObjectIds = new();

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObject(_tunnelIds);

            reader.PersistListWithUInt32Count(
                _containedObjectIds,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistObjectIDValue(ref item);
                });

            var tunnelCount = (uint)_tunnelIds.Count;
            reader.PersistUInt32(ref tunnelCount);
            if (tunnelCount != _tunnelIds.Count)
            {
                throw new InvalidStateException();
            }
        }
    }

    public sealed class PlayerScoreManager : IPersistableObject
    {
        private uint _suppliesCollected;
        private uint _moneySpent;

        private uint[] _numUnitsDestroyedPerPlayer;
        private uint _numUnitsBuilt;
        private uint _numUnitsLost;

        private uint[] _numBuildingsDestroyedPerPlayer;
        private uint _numBuildingsBuilt;
        private uint _numBuildingsLost;

        private uint _numObjectsCaptured;

        private uint _playerId;

        private readonly PlayerStatObjectCollection _objectsBuilt = new();
        private readonly PlayerStatObjectCollection[] _objectsDestroyedPerPlayer;
        private readonly PlayerStatObjectCollection _objectsLost = new();
        private readonly PlayerStatObjectCollection _objectsCaptured = new();

        internal PlayerScoreManager()
        {
            _numUnitsDestroyedPerPlayer = new uint[Player.MaxPlayers];

            _numBuildingsDestroyedPerPlayer = new uint[Player.MaxPlayers];

            _objectsDestroyedPerPlayer = new PlayerStatObjectCollection[Player.MaxPlayers];
            for (var i = 0; i < _objectsDestroyedPerPlayer.Length; i++)
            {
                _objectsDestroyedPerPlayer[i] = new PlayerStatObjectCollection();
            }
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref _suppliesCollected);
            reader.PersistUInt32(ref _moneySpent);

            reader.PersistArray(
                _numUnitsDestroyedPerPlayer,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistUInt32Value(ref item);
                });

            reader.PersistUInt32(ref _numUnitsBuilt);
            reader.PersistUInt32(ref _numUnitsLost);

            reader.PersistArray(
                _numBuildingsDestroyedPerPlayer,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistUInt32Value(ref item);
                });

            reader.PersistUInt32(ref _numBuildingsBuilt);
            reader.PersistUInt32(ref _numBuildingsLost);
            reader.PersistUInt32(ref _numObjectsCaptured);

            reader.SkipUnknownBytes(8);

            reader.PersistUInt32(ref _playerId);
            reader.PersistObject(_objectsBuilt);

            reader.PersistArrayWithUInt16Length(
                _objectsDestroyedPerPlayer,
                static (StatePersister persister, ref PlayerStatObjectCollection item) =>
                {
                    persister.PersistObjectValue(item);
                });

            reader.PersistObject(_objectsLost);
            reader.PersistObject(_objectsCaptured);
        }
    }
}

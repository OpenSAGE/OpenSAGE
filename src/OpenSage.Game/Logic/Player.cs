using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic
{
    [DebuggerDisplay("[Player: {Name}]")]
    public class Player
    {
        public const int MaxPlayers = 16;

        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly AssetStore _assetStore;

        private readonly SupplyManager _supplyManager;

        private readonly List<Upgrade> _upgrades;
        private readonly StringSet _upgradesInProgress;

        public readonly StringSet UpgradesCompleted;

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
        public uint SkillPointsTotal { get; private set; }
        public uint SkillPointsAvailable { get; set; }
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

        // TODO: Does the order matter? Is it ever visible in UI?
        // TODO: Yes the order does matter. For example, the sound played when moving mixed groups of units is the one for the most-recently-selected unit.
        private HashSet<GameObject> _selectedUnits;
        public IReadOnlyCollection<GameObject> SelectedUnits => _selectedUnits;

        public GameObject HoveredUnit { get; set; }

        public int Team { get; init; }

        public Player(uint id, PlayerTemplate template, in ColorRgb color, AssetStore assetStore)
        {
            Id = id;
            Template = template;
            Color = color;
            _selectedUnits = new HashSet<GameObject>();
            Allies = new HashSet<Player>();
            Enemies = new HashSet<Player>();

            _supplyManager = new SupplyManager();

            _upgrades = new List<Upgrade>();
            _upgradesInProgress = new StringSet();
            UpgradesCompleted = new StringSet();

            _sciences = new ScienceSet(assetStore);
            _sciencesDisabled = new ScienceSet(assetStore);
            _sciencesHidden = new ScienceSet(assetStore);

            _teamTemplates = new List<TeamTemplate>();

            _assetStore = assetStore;

            Rank = new Rank(this, assetStore.Ranks);

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
                    _sciences.Add(science.Value.Name, science.Value);
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

        public bool ScienceAvailable(Science science)
        {
            if (HasScience(science))
            {
                return false;
            }

            if (_sciencesDisabled.ContainsKey(science.Name))
            {
                return false;
            }

            if (_sciencesHidden.ContainsKey(science.Name))
            {
                return false;
            }

            foreach (var requiredScience in science.PrerequisiteSciences)
            {
                if (requiredScience.Value == null)
                {
                    continue;
                }

                if (!_sciences.ContainsKey(requiredScience.Value.Name))
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
            _sciences.Add(science.Name, science);
        }

        public bool HasScience(Science science)
        {
            return _sciences.ContainsKey(science.Name);
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
                    _upgradesInProgress.Add(template.Name);
                    break;

                case UpgradeStatus.Completed:
                    _upgradesInProgress.Remove(template.Name);
                    UpgradesCompleted.Add(template.Name);
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

        internal bool HasUpgrade(UpgradeTemplate template)
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

        internal void Load(SaveFileReader reader, Game game)
        {
            reader.ReadVersion(8);

            BankAccount.Load(reader);

            var upgradeQueueCount = (ushort)_upgrades.Count;
            reader.ReadUInt16(ref upgradeQueueCount);

            reader.SkipUnknownBytes(1);

            _sciencesDisabled.Load(reader);
            _sciencesHidden.Load(reader);

            for (var i = 0; i < upgradeQueueCount; i++)
            {
                var upgradeName = "";
                reader.ReadAsciiString(ref upgradeName);
                var upgradeTemplate = reader.AssetStore.Upgrades.GetByName(upgradeName);

                // Use UpgradeStatus.Invalid temporarily because we're going to load the
                // actual queued / completed status below.
                var upgrade = AddUpgrade(upgradeTemplate, UpgradeStatus.Invalid);

                upgrade.Load(reader);
            }

            _unknown1 = reader.ReadUInt32();

            reader.ReadBoolean(ref _unknown2);

            _unknown3 = reader.ReadUInt32();

            reader.ReadBoolean(ref _hasInsufficientPower);

            _upgradesInProgress.Load(reader);
            UpgradesCompleted.Load(reader);

            {
                reader.ReadVersion(2);

                var playerId = reader.ReadUInt32();
                if (playerId != Id)
                {
                    throw new InvalidStateException();
                }
            }

            var numTeamTemplates = (ushort) _teamTemplates.Count;
            reader.ReadUInt16(ref numTeamTemplates);

            for (var i = 0; i < numTeamTemplates; i++)
            {
                var teamTemplateId = reader.ReadUInt32();
                var teamTemplate = game.Scene3D.TeamFactory.FindTeamTemplateById(teamTemplateId);
                if (teamTemplate.Owner != this)
                {
                    throw new InvalidStateException();
                }
                _teamTemplates.Add(teamTemplate);
            }

            var buildListItemCount = (ushort) _buildListItems.Count;
            reader.ReadUInt16(ref buildListItemCount);

            for (var i = 0; i < buildListItemCount; i++)
            {
                var buildListItem = new BuildListItem();
                buildListItem.Load(reader);
                _buildListItems.Add(buildListItem);
            }

            var isAIPlayer = AIPlayer != null;
            reader.ReadBoolean(ref isAIPlayer);
            if (isAIPlayer != (AIPlayer != null))
            {
                throw new InvalidStateException();
            }
            if (isAIPlayer)
            {
                AIPlayer.Load(reader);
            }

            var hasSupplyManager = _supplyManager != null;
            reader.ReadBoolean(ref hasSupplyManager);
            if (hasSupplyManager)
            {
                _supplyManager.Load(reader);
            }

            var hasTunnelManager = _tunnelManager != null;
            reader.ReadBoolean(ref hasTunnelManager);
            if (hasTunnelManager)
            {
                _tunnelManager = new TunnelManager();
                _tunnelManager.Load(reader);
            }

            var defaultTeamId = reader.ReadUInt32();
            DefaultTeam = game.Scene3D.TeamFactory.FindTeamById(defaultTeamId);
            if (DefaultTeam.Template.Owner != this)
            {
                throw new InvalidStateException();
            }

            _sciences.Load(reader);

            var rankId = reader.ReadUInt32();
            Rank.SetRank((int) rankId);
            SkillPointsTotal = reader.ReadUInt32();
            SkillPointsAvailable = reader.ReadUInt32();

            _unknown4 = reader.ReadUInt32(); // 800
            _unknown5 = reader.ReadUInt32(); // 0

            reader.ReadUnicodeString(ref Name);

            _playerToPlayerRelationships.Load(reader);
            _playerToTeamRelationships.Load(reader);

            reader.ReadBoolean(ref CanBuildUnits);
            reader.ReadBoolean(ref CanBuildBuildings);
            reader.ReadBoolean(ref _unknown6);
            reader.ReadSingle(ref GeneralsExperienceMultiplier);
            reader.ReadBoolean(ref ShowOnScoreScreen);

            for (var i = 0; i < _attackedByPlayerIds.Length; i++)
            {
                reader.ReadBoolean(ref _attackedByPlayerIds[i]);
            }

            reader.SkipUnknownBytes(70);

            _scoreManager.Load(reader);

            reader.SkipUnknownBytes(4);

            var numControlGroups = (ushort)_controlGroups.Count;
            reader.ReadUInt16(ref numControlGroups);

            for (var i = 0; i < numControlGroups; i++)
            {
                var controlGroup = new ObjectIdSet();
                controlGroup.Load(reader);

                _controlGroups.Add(controlGroup);
            }

            var unknown = true;
            reader.ReadBoolean(ref unknown);
            if (!unknown)
            {
                throw new InvalidStateException();
            }

            _destroyedObjects.Load(reader);

            reader.SkipUnknownBytes(14);
        }

        public static Player FromMapData(uint index, Data.Map.Player mapPlayer, AssetStore assetStore, bool isSkirmish)
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

            var result = new Player(index, template, color, assetStore)
            {
                Side = side,
                Name = name,
                DisplayName = translatedDisplayName,
                IsHuman = isHuman,
            };

            result.AIPlayer = isHuman || template == null || side == "FactionObserver"
                ? null
                : (isSkirmish && side != "FactionCivilian" ? new SkirmishAIPlayer(result) : new AIPlayer(result));

            if (template != null)
            {
                result.BankAccount.Money = (uint) (template.StartMoney + assetStore.GameData.Current.DefaultStartingCash);
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

    public class AIPlayer
    {
        private readonly Player _owner;

        private readonly List<AIPlayerUnknownThing> _unknownThings = new();
        private readonly List<AIPlayerUnknownThing> _unknownThings2 = new();
        private bool _unknownBool1;
        private bool _unknownBool2;
        private uint _unknownInt1;
        private int _unknownInt2;
        private uint _unknownInt3;
        private uint _unknownInt4;
        private uint _unknownObjectId;
        private uint _unknownInt5;
        private uint _unknownInt6;
        private int _unknownInt7;
        private Vector3 _unknownPosition;
        private bool _unknownBool3;
        private float _unknownFloat;

        internal AIPlayer(Player owner)
        {
            _owner = owner;
        }

        internal virtual void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownCount = (ushort) _unknownThings.Count;
            reader.ReadUInt16(ref unknownCount);

            for (var i = 0; i < unknownCount; i++)
            {
                var thing = new AIPlayerUnknownThing();
                thing.Load(reader);
                _unknownThings.Add(thing);
            }

            var unknownCount2 = (ushort) _unknownThings2.Count;
            reader.ReadUInt16(ref unknownCount2);

            for (var i = 0; i < unknownCount2; i++)
            {
                var thing = new AIPlayerUnknownThing();
                thing.Load(reader);
                _unknownThings2.Add(thing);
            }

            var playerId = reader.ReadUInt32();
            if (playerId != _owner.Id)
            {
                throw new InvalidStateException();
            }

            reader.ReadBoolean(ref _unknownBool1);
            reader.ReadBoolean(ref _unknownBool2);

            _unknownInt1 = reader.ReadUInt32();
            if (_unknownInt1 != 2 && _unknownInt1 != 0)
            {
                throw new InvalidStateException();
            }

            reader.ReadInt32(ref _unknownInt2);
            if (_unknownInt2 != 0 && _unknownInt2 != -1)
            {
                throw new InvalidStateException();
            }

            _unknownInt3 = reader.ReadUInt32(); // 50, 51, 8, 35

            _unknownInt4 = reader.ReadUInt32(); // 0, 50

            var unknown6 = reader.ReadUInt32();
            if (unknown6 != 10)
            {
                throw new InvalidDataException();
            }

            reader.ReadObjectID(ref _unknownObjectId);

            _unknownInt5 = reader.ReadUInt32(); // 0, 1

            _unknownInt6 = reader.ReadUInt32();
            if (_unknownInt6 != 1 && _unknownInt6 != 0 && _unknownInt6 != 2)
            {
                throw new InvalidStateException();
            }

            reader.ReadInt32(ref _unknownInt7);
            if (_unknownInt7 != -1 && _unknownInt7 != 0 && _unknownInt7 != 1)
            {
                throw new InvalidStateException();
            }

            reader.ReadVector3(ref _unknownPosition);
            reader.ReadBoolean(ref _unknownBool3);
            reader.ReadSingle(ref _unknownFloat);

            reader.SkipUnknownBytes(22);
        }

        private sealed class AIPlayerUnknownThing
        {
            private readonly List<AIPlayerUnknownOtherThing> _unknownThings = new();
            private bool _unknownBool;
            private uint _unknownInt1;
            private uint _unknownInt2;

            internal void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                var count = (ushort) _unknownThings.Count;
                reader.ReadUInt16(ref count);

                for (var i = 0; i < count; i++)
                {
                    var otherThing = new AIPlayerUnknownOtherThing();
                    otherThing.Load(reader);
                    _unknownThings.Add(otherThing);
                }

                reader.ReadBoolean(ref _unknownBool);

                _unknownInt1 = reader.ReadUInt32(); // 11

                _unknownInt2 = reader.ReadUInt32();

                reader.SkipUnknownBytes(7);
            }
        }

        private sealed class AIPlayerUnknownOtherThing
        {
            private string _objectName;
            private uint _objectId;
            private uint _unknownInt1;
            private uint _unknownInt2;
            private bool _unknownBool1;
            private bool _unknownBool2;

            internal void Load(SaveFileReader reader)
            {
                reader.ReadVersion(1);

                reader.ReadAsciiString(ref _objectName);
                reader.ReadObjectID(ref _objectId);

                _unknownInt1 = reader.ReadUInt32(); // 0
                _unknownInt2 = reader.ReadUInt32(); // 1

                reader.ReadBoolean(ref _unknownBool1);
                reader.ReadBoolean(ref _unknownBool2);
            }
        }
    }

    public sealed class SkirmishAIPlayer : AIPlayer
    {
        private int _unknownInt1;
        private int _unknownInt2;
        private float _unknownFloat1;
        private float _unknownFloat2;

        internal SkirmishAIPlayer(Player owner)
            : base(owner)
        {

        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadInt32(ref _unknownInt1);
            reader.ReadInt32(ref _unknownInt2);
            reader.ReadSingle(ref _unknownFloat1);
            reader.ReadSingle(ref _unknownFloat2);

            reader.SkipUnknownBytes(16);
        }
    }

    public sealed class SupplyManager
    {
        private readonly ObjectIdSet _supplyWarehouses;
        private readonly ObjectIdSet _supplyCenters;

        internal SupplyManager()
        {
            _supplyWarehouses = new ObjectIdSet();
            _supplyCenters = new ObjectIdSet();
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            _supplyWarehouses.Load(reader);
            _supplyCenters.Load(reader);
        }
    }

    public enum RelationshipType : uint
    {
        Enemies = 0,
        Neutral = 1,
        Allies = 2,
    }

    public sealed class PlayerRelationships
    {
        private readonly Dictionary<uint, RelationshipType> _store = new();

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            _store.Clear();

            var count = (ushort)_store.Count;
            reader.ReadUInt16(ref count);

            for (var i = 0; i < count; i++)
            {
                var playerOrTeamId = reader.ReadUInt32();

                RelationshipType relationship = default;
                reader.ReadEnum(ref relationship);

                _store[playerOrTeamId] = relationship;
            }
        }
    }

    public sealed class ScienceSet : Dictionary<string, Science>
    {
        private readonly AssetStore _assetStore;

        internal ScienceSet(AssetStore assetStore)
        {
            _assetStore = assetStore;
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Clear();

            var count = (ushort) Count;
            reader.ReadUInt16(ref count);

            for (var i = 0; i < count; i++)
            {
                var name = "";
                reader.ReadAsciiString(ref name);

                var science = _assetStore.Sciences.GetByName(name);

                Add(name, science);
            }
        }
    }

    // TODO: I don't know if these are always serialized the same way in .sav files.
    // Maybe we shouldn't use a generic container like this.
    public sealed class StringSet : HashSet<string>
    {
        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Clear();

            var count = (ushort) Count;
            reader.ReadUInt16(ref count);

            for (var i = 0; i < count; i++)
            {
                var item = "";
                reader.ReadAsciiString(ref item);
                Add(item);
            }
        }
    }

    // TODO: I don't know if these are always serialized the same way in .sav files.
    // Maybe we shouldn't use a generic container like this.
    public sealed class ObjectIdSet : HashSet<uint>
    {
        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Clear();

            var count = (ushort) Count;
            reader.ReadUInt16(ref count);

            for (var i = 0; i < count; i++)
            {
                Add(reader.ReadUInt32());
            }
        }
    }

    internal sealed class PlayerStats
    {
        public readonly PlayerStatObjectCollection UnitsDestroyed = new PlayerStatObjectCollection();

        internal void Load(SaveFileReader reader)
        {
            // After 0x10, 3rd entry is ObjectsDestroyed?
            // After 0x10, 17th entry is ObjectsLost?
            UnitsDestroyed.Load(reader);
        }
    }

    internal sealed class PlayerStatObjectCollection : Dictionary<string, uint>
    {
        internal void Load(SaveFileReader reader)
        {
            Clear();

            reader.ReadVersion(1);

            var count = (ushort) Count;
            reader.ReadUInt16(ref count);

            for (var i = 0; i < count; i++)
            {
                var objectType = "";
                reader.ReadAsciiString(ref objectType);

                var total = reader.ReadUInt32();

                Add(objectType, total);
            }
        }
    }

    public enum UpgradeStatus
    {
        Invalid = 0,
        Queued = 1,
        Completed = 2
    }

    public sealed class BankAccount
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public uint Money { get; internal set; }

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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Money = reader.ReadUInt32();
        }
    }

    public sealed class TunnelManager
    {
        private readonly ObjectIdSet _tunnelIds = new();
        private readonly List<uint> _containedObjectIds = new();

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            _tunnelIds.Load(reader);

            var containedCount = reader.ReadUInt32();
            for (var i = 0; i < containedCount; i++)
            {
                uint containedObjectId = 0;
                reader.ReadObjectID(ref containedObjectId);
                _containedObjectIds.Add(containedObjectId);
            }

            var tunnelCount = reader.ReadUInt32();
            if (tunnelCount != _tunnelIds.Count)
            {
                throw new InvalidStateException();
            }
        }
    }

    public sealed class PlayerScoreManager
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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            _suppliesCollected = reader.ReadUInt32();
            _moneySpent = reader.ReadUInt32();

            for (var i = 0; i < _numUnitsDestroyedPerPlayer.Length; i++)
            {
                _numUnitsDestroyedPerPlayer[i] = reader.ReadUInt32();
            }

            _numUnitsBuilt = reader.ReadUInt32();

            _numUnitsLost = reader.ReadUInt32();

            for (var i = 0; i < _numBuildingsDestroyedPerPlayer.Length; i++)
            {
                _numBuildingsDestroyedPerPlayer[i] = reader.ReadUInt32();
            }

            _numBuildingsBuilt = reader.ReadUInt32();

            _numBuildingsLost = reader.ReadUInt32();

            _numObjectsCaptured = reader.ReadUInt32();

            reader.SkipUnknownBytes(8);

            _playerId = reader.ReadUInt32();

            _objectsBuilt.Load(reader);

            var numObjectsDestroyedPerPlayer = (ushort) _objectsDestroyedPerPlayer.Length;
            reader.ReadUInt16(ref numObjectsDestroyedPerPlayer);
            if (numObjectsDestroyedPerPlayer != _objectsDestroyedPerPlayer.Length)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < _objectsDestroyedPerPlayer.Length; i++)
            {
                _objectsDestroyedPerPlayer[i].Load(reader);
            }

            _objectsLost.Load(reader);

            _objectsCaptured.Load(reader);
        }
    }
}

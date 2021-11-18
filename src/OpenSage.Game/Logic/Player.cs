using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Data.Map;
using OpenSage.Data.Sav;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic
{
    [DebuggerDisplay("[Player: {Name}]")]
    public class Player
    {
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

        public uint Id { get; }
        public PlayerTemplate Template { get; }
        public string Name { get; internal set; }
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

            var upgradeQueueCount = reader.ReadUInt16();

            if (reader.ReadBoolean())
            {
                throw new InvalidDataException();
            }

            _sciencesDisabled.Load(reader);
            _sciencesHidden.Load(reader);

            for (var i = 0; i < upgradeQueueCount; i++)
            {
                var upgradeName = reader.ReadAsciiString();
                var upgradeTemplate = _assetStore.Upgrades.GetByName(upgradeName);

                // Use UpgradeStatus.Invalid temporarily because we're going to load the
                // actual queued / completed status below.
                var upgrade = AddUpgrade(upgradeTemplate, UpgradeStatus.Invalid);

                upgrade.Load(reader);
            }

            reader.__Skip(9);

            var hasInsufficientPower = reader.ReadBoolean();

            _upgradesInProgress.Load(reader);
            UpgradesCompleted.Load(reader);

            if (reader.ReadByte() != 2)
            {
                throw new InvalidDataException();
            }

            var playerId = reader.ReadUInt32();
            if (playerId != Id)
            {
                throw new InvalidDataException();
            }

            var numTeamTemplates = reader.ReadUInt16();
            for (var i = 0; i < numTeamTemplates; i++)
            {
                var teamTemplateId = reader.ReadUInt32();
                var teamTemplate = game.Scene3D.TeamFactory.FindTeamTemplateById(teamTemplateId);
                if (teamTemplate.Owner != this)
                {
                    throw new InvalidDataException();
                }
                _teamTemplates.Add(teamTemplate);
            }

            var buildListItemCount = reader.ReadUInt16();
            var buildListItems = new BuildListItem[buildListItemCount];
            for (var i = 0; i < buildListItemCount; i++)
            {
                buildListItems[i] = new BuildListItem();
                buildListItems[i].Load(reader);
            }

            var isAIPlayer = reader.ReadBoolean();
            if (isAIPlayer != (AIPlayer != null))
            {
                throw new InvalidDataException();
            }
            if (isAIPlayer)
            {
                AIPlayer.Load(reader);
            }

            var hasSupplyManager = reader.ReadBoolean();
            if (hasSupplyManager)
            {
                _supplyManager.Load(reader);
            }

            var somePlayerType = reader.ReadBoolean();
            if (somePlayerType)
            {
                reader.ReadVersion(1);

                var tunnels = new ObjectIdSet();
                tunnels.Load(reader);

                var containedCount = reader.ReadUInt32();
                for (var i = 0; i < containedCount; i++)
                {
                    var containedObjectId = reader.ReadObjectID();
                }

                var tunnelCount = reader.ReadUInt32();
                if (tunnelCount != tunnels.Count)
                {
                    throw new InvalidDataException();
                }
            }

            var defaultTeamId = reader.ReadUInt32();
            DefaultTeam = game.Scene3D.TeamFactory.FindTeamById(defaultTeamId);
            if (DefaultTeam.Template.Owner != this)
            {
                throw new InvalidDataException();
            }

            _sciences.Load(reader);

            var rankId = reader.ReadUInt32();
            Rank.SetRank((int) rankId);
            SkillPointsTotal = reader.ReadUInt32();
            SkillPointsAvailable = reader.ReadUInt32();

            var unknown4 = reader.ReadUInt32(); // 800
            var unknown5 = reader.ReadUInt32(); // 0

            Name = reader.ReadUnicodeString();

            _playerToPlayerRelationships.Load(reader);
            _playerToTeamRelationships.Load(reader);

            CanBuildUnits = reader.ReadBoolean();
            CanBuildBuildings = reader.ReadBoolean();

            var unknown6 = reader.ReadBoolean();

            GeneralsExperienceMultiplier = reader.ReadSingle();
            ShowOnScoreScreen = reader.ReadBoolean();

            reader.__Skip(87);

            var suppliesCollected = reader.ReadUInt32();
            var moneySpent = reader.ReadUInt32();

            reader.__Skip(156);

            var unknown8 = reader.ReadUInt32();

            var buildingsCreated = new PlayerStatObjectCollection();
            buildingsCreated.Load(reader);

            var numPlayers = reader.ReadUInt16();
            for (var i = 0; i < numPlayers; i++)
            {
                var playerObjectsDestroyed = new PlayerStatObjectCollection();
                playerObjectsDestroyed.Load(reader);
            }

            var unitsCreated = new PlayerStatObjectCollection();
            unitsCreated.Load(reader);

            var buildingsCaptured = new PlayerStatObjectCollection();
            buildingsCaptured.Load(reader);

            var unknown11 = reader.ReadUInt32();
            if (unknown11 != 0)
            {
                throw new InvalidDataException();
            }

            var numControlGroups = reader.ReadUInt16();
            for (var i = 0; i < numControlGroups; i++)
            {
                var controlGroup = new ObjectIdSet();
                controlGroup.Load(reader);
            }

            if (!reader.ReadBoolean())
            {
                throw new InvalidDataException();
            }

            var destroyedObjects = new ObjectIdSet();
            destroyedObjects.Load(reader);

            reader.__Skip(14);
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

        internal AIPlayer(Player owner)
        {
            _owner = owner;
        }

        internal virtual void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownCount = reader.ReadUInt16();
            for (var i = 0; i < unknownCount; i++)
            {
                reader.ReadVersion(1);

                var unknownInt3 = reader.ReadUInt16();
                if (unknownInt3 != 1)
                {
                    throw new InvalidDataException();
                }

                reader.ReadBoolean();

                var objectName = reader.ReadAsciiString();

                var objectId = reader.ReadObjectID();

                var unknownInt1 = reader.ReadUInt32(); // 0
                var unknownInt2 = reader.ReadUInt32(); // 1

                var unknownBool5 = reader.ReadBoolean();
                var unknownBool6 = reader.ReadBoolean();
                var unknownBool7 = reader.ReadBoolean();

                var unknownInt2_1 = reader.ReadUInt32(); // 11

                for (var j = 0; j < 11; j++)
                {
                    var unknownByte = reader.ReadByte();
                    if (unknownByte != 0)
                    {
                        throw new InvalidDataException();
                    }
                }
            }

            var unknown1 = reader.ReadUInt16();
            if (unknown1 != 0)
            {
                throw new InvalidDataException();
            }

            var playerId = reader.ReadUInt32();
            if (playerId != _owner.Id)
            {
                throw new InvalidDataException();
            }

            var unknownBool1 = reader.ReadBoolean();
            var unknownBool2 = reader.ReadBoolean();

            var unknown2 = reader.ReadUInt32();
            if (unknown2 != 2 && unknown2 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown3 = reader.ReadInt32();
            if (unknown3 != 0 && unknown3 != -1)
            {
                throw new InvalidDataException();
            }

            var unknown4 = reader.ReadUInt32();
            if (unknown4 != 50 && unknown4 != 51 && unknown4 != 8 && unknown4 != 35)
            {
                //throw new InvalidDataException();
            }

            var unknown5 = reader.ReadUInt32();
            if (unknown5 != 0 && unknown5 != 50)
            {
                throw new InvalidDataException();
            }

            var unknown6 = reader.ReadUInt32();
            if (unknown6 != 10)
            {
                throw new InvalidDataException();
            }

            var unknown7 = reader.ReadObjectID();

            var unknown8 = reader.ReadUInt32();
            if (unknown8 != 0 && unknown8 != 1)
            {
                throw new InvalidDataException();
            }

            var unknown9 = reader.ReadUInt32();
            if (unknown9 != 1 && unknown9 != 0 && unknown9 != 2)
            {
                throw new InvalidDataException();
            }

            var unknown10 = reader.ReadInt32();
            if (unknown10 != -1 && unknown10 != 0 && unknown10 != 1)
            {
                throw new InvalidDataException();
            }

            var unknownPosition = reader.ReadVector3();
            var unknownBool8 = reader.ReadBoolean();
            var unknownFloat = reader.ReadSingle();

            for (var i = 0; i < 22; i++)
            {
                var unknownByte = reader.ReadByte();
                if (unknownByte != 0)
                {
                    throw new InvalidDataException();
                }
            }
        }
    }

    public sealed class SkirmishAIPlayer : AIPlayer
    {
        internal SkirmishAIPlayer(Player owner)
            : base(owner)
        {

        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            for (var i = 0; i < 32; i++)
            {
                var unknownByte = reader.ReadByte();
                if (unknownByte != 0)
                {
                    throw new InvalidDataException();
                }
            }
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
        private readonly Dictionary<uint, RelationshipType> _store;

        public PlayerRelationships()
        {
            _store = new Dictionary<uint, RelationshipType>();
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            _store.Clear();

            var count = reader.ReadUInt16();
            for (var i = 0; i < count; i++)
            {
                var playerOrTeamId = reader.ReadUInt32();
                var relationship = reader.ReadEnum<RelationshipType>();
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

            var count = reader.ReadUInt16();
            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadAsciiString();

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

            var count = reader.ReadUInt16();
            for (var i = 0; i < count; i++)
            {
                Add(reader.ReadAsciiString());
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

            var count = reader.ReadUInt16();
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

            var count = reader.ReadUInt16();
            for (var i = 0; i < count; i++)
            {
                var objectType = reader.ReadAsciiString();
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
}

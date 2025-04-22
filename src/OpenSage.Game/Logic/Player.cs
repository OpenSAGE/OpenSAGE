#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenSage.Content.Translation;
using OpenSage.Data.Map;
using OpenSage.Logic.AI;
using OpenSage.Logic.Map;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Network;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic;

[DebuggerDisplay("[Player: {Name}]")]
public class Player : IPersistableObject
{
    public const int MaxPlayers = 16;
    public const int NumHotkeySquads = 10;

    private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IGame _game;

    public readonly SupplyManager SupplyManager;

    private readonly List<Upgrade> _upgrades;
    private readonly UpgradeSet _upgradesInProgress;

    public readonly UpgradeSet UpgradesCompleted;

    private readonly ScienceSet _sciences;
    private readonly ScienceSet _sciencesDisabled;
    private readonly ScienceSet _sciencesHidden;

    private readonly PlayerRelationships<PlayerIndex> _playerToPlayerRelationships = new();
    private readonly PlayerRelationships<TeamId> _playerToTeamRelationships = new();

    private readonly List<TeamPrototype> _teamPrototypes;

    private int _radarCount;
    private bool _isPlayerDead;
    private int _disableProofRadarCount;
    private bool _radarDisabled;

    private BuildListInfo[] _buildList = [];
    public BuildListInfo[] BuildList { get => _buildList; set => _buildList = value; }

    private TunnelTracker? _tunnelSystem;
    public TunnelTracker? TunnelManager => _tunnelSystem;
    private uint _levelUp;
    private uint _levelDown;
    private bool _observer;
    private readonly bool[] _attackedByPlayerIds = new bool[MaxPlayers];
    private readonly ScoreKeeper _scoreKeeper = new();
    internal readonly SyncedSpecialPowerTimerCollection SyncedSpecialPowerTimers = [];
    private readonly Squad[] _squads = new Squad[NumHotkeySquads];
    private readonly ObjectIdSet _destroyedObjects = new();

    private int _bombardBattlePlans;
    private int _holdTheLineBattlePlans;
    private int _searchAndDestroyBattlePlans;

    private bool _unitsShouldHunt;

    private StrategyData? _strategyData;

    public PlayerIndex Index { get; }
    public PlayerTemplate? Template { get; private set; }
    public string? Name;
    public string? DisplayName { get; private set; }

    public string? Side { get; private set; }
    public string? BaseSide { get; private set; }

    public bool IsHuman { get; private set; }

    public Team? DefaultTeam { get; private set; }

    public BankAccount BankAccount;

    public Rank Rank { get; set; }
    public uint SkillPointsTotal;
    public uint SkillPointsAvailable;
    public uint SciencePurchasePoints { get; set; }
    public bool CanBuildUnits;
    public bool CanBuildBase;
    public float SkillPointsModifier;
    public bool ListInScoreScreen;

    public AIPlayer? AIPlayer { get; private set; }

    public AcademyStats AcademyStats { get; }

    // TODO(Port): Implement this.
    public bool IsLogicalRetaliationModeEnabled { get; set; }

    private bool _isPreorder;
    private int _mpStartIndex;
    private readonly Handicap _handicap;
    private Squad? _currentSelection;

    // TODO: Should this be derived from the player's buildings so that it doesn't get out of sync?
    public int GetEnergy(IGameObjectCollection allGameObjects)
    {
        var energy = 0;
        foreach (var gameObject in allGameObjects.Objects)
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
        foreach (var requirement in specialPower.RequiredSciences)
        {
            if (!HasScience(requirement.Value))
            {
                return false;
            }
        }

        return true;
    }

    public ColorRgb Color { get; private set; }
    public ColorRgb NightColor { get; private set; }

    public HashSet<Player> Allies { get; internal set; }

    public HashSet<Player> Enemies { get; internal set; }

    private HashSet<GameObject>[] _selectionGroups;

    // TODO: Does the order matter? Is it ever visible in UI?
    // TODO: Yes the order does matter. For example, the sound played when moving mixed groups of units is the one for the most-recently-selected unit.
    private HashSet<GameObject> _selectedUnits;

    private PlayerType _playerType;
    private ResourceGatheringManager? _resourceGatheringManager;

    public IReadOnlyCollection<GameObject> SelectedUnits => _selectedUnits;

    public GameObject? HoveredUnit { get; set; }

    public int Team { get; init; }

    public PlayerMaskType PlayerMask => (PlayerMaskType)(1 << Index.Value);

    public Player(PlayerIndex index, PlayerTemplate? template, in ColorRgb color, IGame game)
    {
        _game = game;

        Index = index;
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

        _teamPrototypes = new List<TeamPrototype>();

        _tunnelSystem = new TunnelTracker(); // todo: one of the map factions in generals doesn't have this - probably ok?

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

        BankAccount = new BankAccount(_game, Index);
        AcademyStats = new AcademyStats(_game);
        _resourceGatheringManager = new ResourceGatheringManager();
        _handicap = new Handicap();
    }

    internal void InitFromDict(Data.Map.Player mapPlayer, GameInfo? gameInfo)
    {
        var sidesList = _game.Scene3D.MapFile.SidesList;

        var templateName = mapPlayer.Faction
            ?? throw new InvalidStateException("Player has no faction.");

        var template = _game.AssetStore.PlayerTemplates.GetByName(templateName) ??
            throw new InvalidStateException($"Player template {templateName} not found.");

        Init(template, gameInfo);

        var playerDisplayName = mapPlayer.DisplayName ?? "";
        var playerName = mapPlayer.Name ?? "";
        Name = playerName;

        var forceHuman = false;
        var skirmish = false;

        if (mapPlayer.IsSkirmish ?? false)
        {
            // Ensure the map actually has a skirmish player.
            // If it doesn't have one, convert another player of the same faction to a skirmish player.

            skirmish = sidesList.Players.Any(p =>
            {
                var faction = p.Faction ?? throw new InvalidStateException("Player has no faction.");
                var factionPlayerTemplate = _game.AssetStore.PlayerTemplates.GetByName(faction);
                return factionPlayerTemplate?.Side == Side;
            });

            if (!skirmish)
            {
                Logger.Warn($"Map has no skirmish player for side {Side}. Converting to skirmish player.");
                forceHuman = true;
            }
        }

        if (mapPlayer.IsHuman ?? false || forceHuman)
        {
            SetPlayerType(PlayerType.Human, false);

            if (mapPlayer.IsPreorder ?? false)
            {
                _isPreorder = true;
            }

            if (sidesList.SkirmishPlayers.Count > 0)
            {
                // Human player gets scripts from the civilian player.

                var civilianPlayer = sidesList.SkirmishPlayers.Find(p =>
                {
                    var faction = p.Faction ?? throw new InvalidStateException("Player has no faction.");
                    var factionPlayerTemplate = _game.AssetStore.PlayerTemplates.GetByName(faction);
                    return factionPlayerTemplate?.Side == "Civilian";
                });

                if (civilianPlayer != null)
                {
                    // BUG from C++: _mpStartIndex is used before it's set. Thefore the qualifier will always be 0, at least
                    // the first time a player is initialized.
                    var qualifier = _mpStartIndex.ToString();
                    var qualTemplatePlayerName = (civilianPlayer.Name ?? "") + _mpStartIndex;

                    var scripts = civilianPlayer.Scripts?.DuplicateAndQualify(
                        qualifier,
                        qualTemplatePlayerName,
                        Name
                    );

                    sidesList.Players[Index.Value].Scripts = scripts;
                    // If I understood the C++ code correctly, the scripts are then removed the from the civilian player.
                    // So scripts attached to the civilian player are moved to the human player.
                    civilianPlayer.Scripts = new Scripting.ScriptList();
                }
            }
            skirmish = false;
        }
        else
        {
            SetPlayerType(PlayerType.Computer, skirmish);
        }

        _mpStartIndex = mapPlayer.MultiplayerStartIndex ?? 0;

        if (skirmish)
        {
            var mySide = Side;

            var skirmishPlayer = sidesList.SkirmishPlayers.Find(p =>
            {
                var playerTemplateName = p.Faction ?? throw new InvalidStateException("Player has no faction.");
                // TODO: This is a hack. We should port PlayerTemplateStore and centralize the logic there (there are many other exceptions)
                var fixedPlayerTemplateName = playerTemplateName == "" ? "FactionCivilian" : playerTemplateName;
                var playerTemplate = _game.AssetStore.PlayerTemplates.GetByName(playerTemplateName == "" ? "FactionCivilian" : playerTemplateName) ??
                    throw new InvalidStateException($"Player template {playerTemplateName} not found.");
                return playerTemplate.Side == mySide;
            });

            var difficultyInt = mapPlayer.SkirmishDifficulty;
            // TODO(Port): initialize difficulty from TheScriptEngine->getGlobalDifficulty
            var difficulty = Difficulty.Normal;

            if (difficultyInt.HasValue)
            {
                difficulty = (Difficulty)difficultyInt.Value;
            }

            if (AIPlayer != null)
            {
                AIPlayer.Difficulty = difficulty;
            }

            if (skirmishPlayer == null)
            {
                throw new InvalidStateException($"Could not find skirmish player for side {mySide}.");
            }

            Name = $"{skirmishPlayer.Name}{_mpStartIndex}";
            var qualifier = _mpStartIndex.ToString();

            var scripts = skirmishPlayer.Scripts?.DuplicateAndQualify(
                qualifier,
                Name,
                Name
            );

            sidesList.Players[Index.Value].Scripts = scripts;

            // Remove all teams owned by this player
            sidesList.Teams.RemoveAll(team => team.Owner == Name);

            // Now add teams
            var originalPlayerName = skirmishPlayer.Name;

            foreach (var skirmishTeam in sidesList.SkirmishTeams)
            {
                if (skirmishTeam.Owner != originalPlayerName)
                {
                    continue;
                }

                var newTeamName = $"{skirmishTeam.Name}{_mpStartIndex}";

                if (sidesList.FindTeamInfo(newTeamName) is not null)
                {
                    continue;
                }

                skirmishTeam.Owner = Name;
                skirmishTeam.Name = newTeamName;

                // Qualify scripts
                Span<string> nameKeys = [
                    TeamKeys.OnCreateScript,
                    TeamKeys.OnIdleScript,
                    TeamKeys.OnUnitDestroyedScript,
                    TeamKeys.OnDestroyedScript,
                    TeamKeys.EnemySightedScript,
                    TeamKeys.AllClearScript,
                    TeamKeys.ProductionCondition
                ];

                foreach (var nameKey in nameKeys)
                {
                    if (!skirmishTeam.Properties.TryGetValue(nameKey, out var property))
                    {
                        continue;
                    }

                    var stringValue = property.GetAsciiString();
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        continue;
                    }

                    property.UpdateValue($"{stringValue}{_mpStartIndex}");
                }

                // Update teamGenericScriptHook keys as well
                for (var i = 0; i < Logic.Team.MaxGenericScripts; i++)
                {
                    var key = $"{TeamKeys.GenericScriptHookPrefix}{i}";
                    if (!skirmishTeam.Properties.TryGetValue(key, out var property))
                    {
                        continue;
                    }

                    var stringValue = property.GetAsciiString();
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        continue;
                    }

                    property.UpdateValue($"{stringValue}{_mpStartIndex}");
                }

                // Done. Now add the team.
                sidesList.Teams.Add(skirmishTeam);
            }
        }

        _resourceGatheringManager = new ResourceGatheringManager();
        _tunnelSystem = new TunnelTracker();
        _handicap.ReadFromDict(mapPlayer.Properties);

        _playerToPlayerRelationships.Clear();
        _playerToTeamRelationships.Clear();

        Array.Fill(_attackedByPlayerIds, false);

        if (mapPlayer.Color is { } color)
        {
            Color = ColorRgb.FromUInt32((uint)color | 0xff000000);
            NightColor = Color;
        }

        if (mapPlayer.NightColor is { } nightColor)
        {
            NightColor = ColorRgb.FromUInt32((uint)nightColor | 0xff000000);
        }

        if (mapPlayer.StartMoney is { } startMoney)
        {
            BankAccount.Deposit((uint)startMoney);
        }

        for (var i = 0; i < _squads.Length; i++)
        {
            _squads[i] = new Squad(_game);
        }

        _currentSelection = new Squad(_game);
    }

    internal void SelectUnits(ICollection<GameObject> units, bool additive = false)
    {
        if (!additive)
        {
            DeselectUnits();
        }

        _selectedUnits.UnionWith(units);

        foreach (var unit in units)
        {
            unit.Drawable.TriggerSelection();
        }

        var unitsFromHordeSelection = new List<GameObject>();
        foreach (var unit in _selectedUnits)
        {
            unit.IsSelected = true;

            if (unit.ParentHorde != null && !unit.ParentHorde.IsSelected)
            {
                unitsFromHordeSelection.Add(unit.ParentHorde);
                unitsFromHordeSelection.AddRange(unit.ParentHorde.FindBehavior<HordeContainBehavior>()?.SelectAll(true) ?? []);
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
            HandleHordeDeselect(unit);
        }
        _selectedUnits.Clear();
    }

    public void DeselectUnit(GameObject unit)
    {
        unit.IsSelected = false;
        HandleHordeDeselect(unit);
        _selectedUnits.Remove(unit);
    }

    private static void HandleHordeDeselect(GameObject unit)
    {
        if (unit.ParentHorde is { IsSelected: true })
        {
            unit.ParentHorde.FindBehavior<HordeContainBehavior>()?.SelectAll(false);
        }
        else
        {
            unit.FindBehavior<HordeContainBehavior>()?.SelectAll(false);
        }
    }

    public void CreateSelectionGroup(int idx)
    {
        if (idx > _selectionGroups.Length)
        {
            Logger.Warn($"Do not support more than {_selectionGroups.Length} groups!");
            return;
        }

        // TODO: when one game object dies we need to remove it from these groups
        _selectionGroups[idx] = new HashSet<GameObject>(_selectedUnits);
    }

    public void SelectGroup(int idx)
    {
        if (idx > _selectionGroups.Length)
        {
            Logger.Warn($"Do not support more than {_selectionGroups.Length} groups!");
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

    /// <summary>
    /// Awards a science to a user without any additional checks or point deductions (e.g. if directly granted by a map, or by being a faction or rank)
    /// </summary>
    public void DirectlyAssignScience(Science science)
    {
        _sciences.Add(science);
        ApplyScienceUpgrades(science);
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

        SciencePurchasePoints -= (uint)science.SciencePurchasePointCost;
        _sciences.Add(science);
        ApplyScienceUpgrades(science);
    }

    private void ApplyScienceUpgrades(Science science)
    {
        foreach (var gameObject in _game.GameLogic.Objects)
        {
            if (gameObject.Owner == this)
            {
                foreach (var upgradableScienceModule in gameObject.FindBehaviors<IUpgradableScienceModule>())
                {
                    upgradableScienceModule.TryUpgrade(science);
                }
            }
        }
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

        // Prerequisites are AND'd.
        foreach (var sciencePrerequisiteList in objectToProduce.Prerequisites.Sciences)
        {
            // The list within each prerequisite is OR'd.

            var hasPrerequisite = false;
            foreach (var science in sciencePrerequisiteList)
            {
                if (HasScience(science.Value))
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
        Upgrade? upgrade = null;
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
        Upgrade? upgradeToRemove = null;

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

    internal void CancelUpgrade(UpgradeTemplate template)
    {
        _upgradesInProgress.Remove(template);
    }

    public bool HasUpgrade(UpgradeTemplate template)
    {
        return UpgradesCompleted.Contains(template);
    }

    internal bool HasEnqueuedUpgrade(UpgradeTemplate template)
    {
        return _upgradesInProgress.Contains(template);
    }

    public void RemoveTeamFromList(TeamPrototype team)
    {
        _teamPrototypes.Remove(team);
    }

    public void AddTeamToList(TeamPrototype team)
    {
        if (_teamPrototypes.Contains(team))
        {
            return;
        }

        _teamPrototypes.Add(team);
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(8);

        reader.PersistObject(BankAccount);

        var upgradeCount = (ushort)_upgrades.Count;
        reader.PersistUInt16(ref upgradeCount);

        reader.PersistBoolean(ref _isPreorder);

        reader.PersistObject(_sciencesDisabled);
        reader.PersistObject(_sciencesHidden);

        reader.BeginArray("Upgrades");
        if (reader.Mode == StatePersistMode.Read)
        {
            for (var i = 0; i < upgradeCount; i++)
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

        reader.PersistInt32(ref _radarCount);
        reader.PersistBoolean(ref _isPlayerDead);
        reader.PersistInt32(ref _disableProofRadarCount);
        reader.PersistBoolean(ref _radarDisabled);

        reader.PersistObject(_upgradesInProgress);
        reader.PersistObject(UpgradesCompleted);

        // TODO(Port): This is Energy
        {
            reader.BeginObject("UnknownThing");

            var unknownThingVersion = reader.PersistVersion(3);

            var playerId = Index;
            reader.PersistPlayerIndex(ref playerId);
            if (playerId != Index)
            {
                throw new InvalidStateException();
            }

            if (unknownThingVersion >= 3)
            {
                reader.SkipUnknownBytes(4);
            }

            reader.EndObject();
        }

        reader.PersistList(
            _teamPrototypes,
            static (StatePersister persister, ref TeamPrototype item) =>
            {
                var teamPrototypeId = (item?.Id)?.Index ?? 0u;
                persister.PersistUInt32Value(ref teamPrototypeId);

                if (persister.Mode == StatePersistMode.Read)
                {
                    var prototype = persister.Game.TeamFactory.FindTeamPrototypeById(new TeamPrototypeId(teamPrototypeId));
                    if (prototype == null)
                    {
                        throw new InvalidStateException();
                    }
                    item = prototype;
                }
            });

        if (reader.Mode == StatePersistMode.Read)
        {
            foreach (var teamPrototype in _teamPrototypes)
            {
                if (teamPrototype.ControllingPlayer != this)
                {
                    throw new InvalidStateException();
                }
            }
        }

        reader.PersistArray(
            _buildList,
            static (StatePersister persister, ref BuildListInfo item) =>
            {
                item ??= new BuildListInfo();
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

        var hasTunnelTracker = _tunnelSystem != null;
        reader.PersistBoolean(ref hasTunnelTracker);
        if (hasTunnelTracker)
        {
            _tunnelSystem ??= new TunnelTracker();
            reader.PersistObject(_tunnelSystem);
        }

        var defaultTeamId = DefaultTeam?.Id ?? TeamId.Invalid;
        reader.PersistTeamId(ref defaultTeamId);
        if (reader.Mode == StatePersistMode.Read)
        {
            DefaultTeam = reader.Game.TeamFactory.FindTeamById(defaultTeamId);
            if (DefaultTeam == null || DefaultTeam.Prototype.ControllingPlayer != this)
            {
                throw new InvalidStateException();
            }
        }

        reader.PersistObject(_sciences);

        var rankId = (uint)Rank.CurrentRank;
        reader.PersistUInt32(ref rankId);
        Rank.SetRank((int)rankId);

        reader.PersistUInt32(ref SkillPointsTotal);
        reader.PersistUInt32(ref SkillPointsAvailable);
        reader.PersistUInt32(ref _levelUp); // 800
        reader.PersistUInt32(ref _levelDown); // 0
        reader.PersistUnicodeString(ref Name);
        reader.PersistObject(_playerToPlayerRelationships);
        reader.PersistObject(_playerToTeamRelationships);
        reader.PersistBoolean(ref CanBuildUnits);
        reader.PersistBoolean(ref CanBuildBase);
        reader.PersistBoolean(ref _observer);
        reader.PersistSingle(ref SkillPointsModifier);
        reader.PersistBoolean(ref ListInScoreScreen);

        reader.PersistArray(
            _attackedByPlayerIds,
            static (StatePersister persister, ref bool item) =>
            {
                persister.PersistBooleanValue(ref item);
            });

        // TODO(Port): _scoreKeeper
        reader.SkipUnknownBytes(4);

        if (reader.SageGame == SageGame.CncGenerals)
        {
            reader.SkipUnknownBytes(66);
        }

        reader.PersistObject(_scoreKeeper);
        reader.SkipUnknownBytes(2);

        reader.PersistObject(SyncedSpecialPowerTimers);

        reader.PersistArray(
            _squads, static (StatePersister persister, ref Squad squad) =>
            {
                persister.PersistObject(squad);
            });

        var currentSelectionPresent = _currentSelection != null;
        reader.PersistBoolean(ref currentSelectionPresent);
        if (currentSelectionPresent)
        {
            if (_currentSelection == null && reader.Mode == StatePersistMode.Read)
            {
                _currentSelection = new Squad(_game);
            }
            reader.PersistObject(_currentSelection);
        }

        reader.PersistObject(_destroyedObjects);

        var hasStrategy = _strategyData != null;
        reader.PersistBoolean(ref hasStrategy);
        if (hasStrategy)
        {
            _strategyData ??= new StrategyData();
            reader.PersistObject(_strategyData);
        }

        reader.PersistInt32(ref _bombardBattlePlans);
        reader.PersistInt32(ref _holdTheLineBattlePlans);
        reader.PersistInt32(ref _searchAndDestroyBattlePlans);
        reader.PersistBoolean(ref _unitsShouldHunt);
    }

    public static Player FromMapData(PlayerIndex index, Data.Map.Player mapPlayer, IGame game, bool isSkirmish)
    {
        var side = mapPlayer.Faction ?? throw new InvalidStateException($"Player {mapPlayer.Name} has no faction.");

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

        var name = mapPlayer.Name;
        var displayName = mapPlayer.DisplayName;
        var translatedDisplayName = displayName.Translate();

        var isHuman = mapPlayer.IsHuman;

        var colorRgb = mapPlayer.Color;

        ColorRgb color;

        if (colorRgb != null)
        {
            color = ColorRgb.FromInt32(colorRgb.Value);
        }
        else if (template != null) // Template is null for the neutral faction
        {
            color = template.PreferredColor;
        }
        else
        {
            color = new ColorRgb(255, 255, 255); // neutral appears to be white
        }

        var result = new Player(index, template, color, game)
        {
            Side = side,
            Name = name,
            DisplayName = translatedDisplayName,
            IsHuman = isHuman ?? false,
        };

        result.AIPlayer = (isHuman ?? false) || template == null || side == "FactionObserver"
            ? null
            : isSkirmish && side != "FactionCivilian"
                ? new AISkirmishPlayer(result)
                : new AIPlayer(result);

        if (template != null)
        {
            result.BankAccount.Money = (uint)(template.StartMoney + game.AssetStore.GameData.Current.DefaultStartingCash);
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

    public void InitializeStrategyData(BitArray<ObjectKinds> validMemberKindOf, BitArray<ObjectKinds> invalidMemberKindOf)
    {
        _strategyData ??= new StrategyData();
        _strategyData.SetMemberKinds(validMemberKindOf, invalidMemberKindOf);
    }

    public void SetActiveBattlePlan(BattlePlanType battlePlan, float armorDamageScalar, float sightRangeScalar)
    {
        // TODO: This is probably not right - these used to be flags, now they are ints, mirroring Generals

        _bombardBattlePlans = battlePlan is BattlePlanType.Bombardment ? 1 : 0;
        _holdTheLineBattlePlans = battlePlan is BattlePlanType.HoldTheLine ? 1 : 0;
        _searchAndDestroyBattlePlans = battlePlan is BattlePlanType.SearchAndDestroy ? 1 : 0;
        _strategyData?.SetActiveBattlePlan(battlePlan, armorDamageScalar, sightRangeScalar);
    }

    public void ClearBattlePlan()
    {
        _bombardBattlePlans = 0;
        _holdTheLineBattlePlans = 0;
        _searchAndDestroyBattlePlans = 0;
        _strategyData?.ClearBattlePlan();
    }

    public RelationshipType GetRelationship(Team? that)
    {
        if (that == null)
        {
            return RelationshipType.Neutral;
        }

        // Check team override
        if (_playerToTeamRelationships.TryGetValue(that.Id, out var relationship))
        {
            return relationship;
        }

        // Check player override
        if (that.ControllingPlayer != null && _playerToPlayerRelationships.TryGetValue(that.ControllingPlayer.Index, out relationship))
        {
            return relationship;
        }

        // Default to neutral
        return RelationshipType.Neutral;
    }

    public void SetAttackedBy(PlayerIndex playerIndex)
    {
        // TODO(Port): Implement this.
    }

    internal void Init(PlayerTemplate? playerTemplate, GameInfo? gameInfo)
    {
        SkillPointsModifier = 1.0f;
        // TODO(Port): _attackedFrame = 0;

        _isPreorder = false;
        _isPlayerDead = false;

        _radarCount = 0;
        _disableProofRadarCount = 0;
        _radarDisabled = false;

        _bombardBattlePlans = 0;
        _holdTheLineBattlePlans = 0;
        _searchAndDestroyBattlePlans = 0;

        // TODO(Port): m_battlePlanBonuses
        // TODO(Port): deleteUpgradeList()
        // TODO(Port): m_energy.init(this);
        // TODO(Port): m_stats.init();

        _buildList = [];
        DefaultTeam = null;

        AIPlayer = null;

        _resourceGatheringManager = null;

        for (var i = 0; i < _squads.Length; i++)
        {
            _squads[i] = new Squad(_game);
        }

        _currentSelection = new Squad(_game);
        _tunnelSystem = new TunnelTracker();

        CanBuildBase = true;
        CanBuildUnits = true;
        _observer = false;

        // TODO(Port): m_cashBountyPercent = 0.0f;

        ListInScoreScreen = true;
        _unitsShouldHunt = false;

        if (playerTemplate != null)
        {
            Side = playerTemplate.Side;
            BaseSide = playerTemplate.BaseSide;
            // TODO(Port): m_productionCostChanges
            // TODO(Port): m_productionTimeChanges
            // TODO(Port): m_productionVeterancyLevels
            Color = playerTemplate.PreferredColor;
            NightColor = Color;

            BankAccount = BankAccount.FromAmount(_game, Index, playerTemplate.StartMoney);
            // TODO(Port): m_handicap

            if (BankAccount.Money == 0)
            {
                if (gameInfo != null)
                {
                    BankAccount = gameInfo.StartingCash;
                } else
                {
                    BankAccount.Money = _game.AssetStore.GameData.Current.DefaultStartingCash;
                }
            }

            DisplayName = "";
            Name = "";
            _playerType = PlayerType.Computer;
            _observer = playerTemplate.IsObserver;
            _isPlayerDead = playerTemplate.IsObserver; // "Observers are dead"
        }
        else
        {
            // No player template means neutral player

            Side = "";
            BaseSide = "";
            // TODO(Port): m_productionCostChanges
            // TODO(Port): m_productionTimeChanges
            // TODO(Port): m_productionVeterancyLevels
            Color = new ColorRgb(255, 255, 255);
            NightColor = new ColorRgb(255, 255, 255);
            BankAccount.Init();
            _handicap.Init();

            DisplayName = "";
            Name = "";
            _playerType = PlayerType.Computer;

            // Neutral player is allied to itself
            SetPlayerRelationship(this, RelationshipType.Allies);
        }

        _scoreKeeper.Reset(Index);
        Template = playerTemplate;

        ResetRank();
        _sciencesDisabled.Clear();
        _sciencesHidden.Clear();

        // TODO(Port): m_specialPowerReadyTimerList handling
        // TODO(Port): m_kindOfPercentProductionChangeList

        AcademyStats.Init(this);

        // TODO(Port): m_logicalRetaliationModeEnabled
    }

    internal void Update()
    {
        // TODO(Port)
    }

    internal void NewMap()
    {
        // TODO(Port)
    }

    internal bool RemoveTeamRelationship(Team team)
    {
        // TODO(Port)
        return false;
    }

    internal void UpdateTeamStates()
    {
        // TODO(Port)
    }

    private void ResetRank()
    {
        // TODO(Port)
    }

    internal void SetPlayerType(PlayerType t, bool skirmish)
    {
        _playerType = t;

        if (t == PlayerType.Computer)
        {
            // TODO(Port): Also enable skirmish AI when TheAI->getAiData()->m_forceSkirmishAI
            if (skirmish)
            {
                AIPlayer = new AISkirmishPlayer(this);
            }
            else
            {
                AIPlayer = new AIPlayer(this);
            }
        }
    }

    internal void SetPlayerRelationship(Player that, RelationshipType r)
    {
        if (that != null)
        {
            _playerToPlayerRelationships[that.Index] = r;
        }
    }

    internal void SetDefaultTeam()
    {
        var teamName = $"team${Name}";
        var team = _game.TeamFactory.FindTeam(teamName);
        if (team != null)
        {
            DefaultTeam = team;
            DefaultTeam.SetActive();
        }
    }
}

public sealed class SupplyManager : IPersistableObject
{
    private readonly IGame _game;

    private readonly ObjectIdSet _supplyWarehouses;
    private readonly ObjectIdSet _supplyCenters;

    internal SupplyManager(IGame game)
    {
        _game = game;

        _supplyWarehouses = new ObjectIdSet();
        _supplyCenters = new ObjectIdSet();
    }

    public void AddSupplyCenter(GameObject supplyCenter)
    {
        _supplyCenters.Add(supplyCenter.Id);
    }

    public void RemoveSupplyCenter(GameObject supplyCenter)
    {
        _supplyCenters.Remove(supplyCenter.Id);
    }

    public void AddSupplyWarehouse(GameObject supplyWarehouse)
    {
        _supplyWarehouses.Add(supplyWarehouse.Id);
    }

    public void RemoveSupplyWarehouse(GameObject supplyWarehouse)
    {
        _supplyWarehouses.Remove(supplyWarehouse.Id);
    }

    public bool Contains(GameObject supplyCenter)
    {
        return _supplyCenters.Contains(supplyCenter.Id);
    }

    public GameObject? FindClosestSupplyCenter(GameObject supplyGatherer)
    {
        GameObject? closestSupplyCenter = null;
        var closestDistanceSquared = float.MaxValue;

        foreach (var supplyCenterId in _supplyCenters)
        {
            var supplyCenter = _game.GameLogic.GetObjectById(supplyCenterId);

            if (supplyCenter == null)
            {
                continue;
            }

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

    public GameObject? FindClosestSupplyWarehouse(GameObject supplyGatherer)
    {
        GameObject? closestSupplyWarehouse = null;
        var closestDistanceSquared = float.MaxValue;

        var supplyAIUpdate = supplyGatherer.FindBehavior<SupplyAIUpdate>();
        if (supplyAIUpdate != null)
        {
            closestDistanceSquared = supplyAIUpdate.SupplyWarehouseScanDistance * supplyAIUpdate.SupplyWarehouseScanDistance;
        }

        foreach (var supplyWarehouseId in _supplyWarehouses)
        {
            var supplyWarehouse = _game.GameLogic.GetObjectById(supplyWarehouseId);

            if (supplyWarehouse == null)
            {
                continue;
            }

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

public sealed class PlayerRelationships<TKey> : IPersistableObject
    where TKey : struct, ISideId<TKey>
{
    private readonly Dictionary<TKey, RelationshipType> _store = [];

    public RelationshipType this[TKey key]
    {
        get => _store[key];
        set => _store[key] = value;
    }

    public bool TryGetValue(TKey key, out RelationshipType relationship)
    {
        return _store.TryGetValue(key, out relationship);
    }

    public void Clear()
    {
        _store.Clear();
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistDictionary(
            _store,
            static (StatePersister persister, ref TKey key, ref RelationshipType value) =>
        {
            key.Persist(ref key, persister, "PlayerOrTeamId");
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

public sealed class ObjectIdSet : HashSet<ObjectId>, IPersistableObject
{
    public void Persist(StatePersister persister)
    {
        persister.PersistVersion(1);

        persister.PersistHashSet(
            this,
            static (StatePersister persister, ref ObjectId item) =>
            {
                persister.PersistObjectIdValue(ref item);
            },
            "Values");
    }
}

public sealed class SyncedSpecialPowerTimerCollection : Dictionary<SpecialPowerType, LogicFrame>, IPersistableObject
{
    public void Persist(StatePersister persister)
    {
        persister.PersistDictionary(this,
            static (StatePersister statePersister, ref SpecialPowerType specialPowerType, ref LogicFrame availableAtFrame) =>
            {
                statePersister.PersistEnum(ref specialPowerType);
                statePersister.PersistLogicFrame(ref availableAtFrame);
            }, "Dictionary");
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

public sealed class TunnelTracker : IPersistableObject
{
    public ObjectIdSet TunnelIds { get; } = [];
    public List<ObjectId> ContainedObjectIds { get; } = [];

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistObject(TunnelIds);

        reader.PersistListWithUInt32Count(
            ContainedObjectIds,
            static (StatePersister persister, ref ObjectId item) =>
            {
                persister.PersistObjectIdValue(ref item);
            });

        var tunnelCount = (uint)TunnelIds.Count;
        reader.PersistUInt32(ref tunnelCount);
        if (tunnelCount != TunnelIds.Count)
        {
            throw new InvalidStateException();
        }
    }
}

public sealed class ScoreKeeper : IPersistableObject
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

    internal ScoreKeeper()
    {
        _numUnitsDestroyedPerPlayer = new uint[Player.MaxPlayers];

        _numBuildingsDestroyedPerPlayer = new uint[Player.MaxPlayers];

        _objectsDestroyedPerPlayer = new PlayerStatObjectCollection[Player.MaxPlayers];
        for (var i = 0; i < _objectsDestroyedPerPlayer.Length; i++)
        {
            _objectsDestroyedPerPlayer[i] = new PlayerStatObjectCollection();
        }
    }

    internal void Reset(PlayerIndex index)
    {
        // TODO(Port)
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

// this seems to be _pretty_ similar to the content of BattlePlanUpdate, but not entirely?
public sealed class StrategyData : IPersistableObject
{
    private bool _bombardmentActive;
    private bool _holdTheLineActive;
    private bool _searchAndDestroyActive;

    private float _activeArmorDamageScalar = 1; // 0.9 when hold the line is active
    private float _activeSightRangeScalar = 1; // 1.2 when search and destroy is active

    private BitArray<ObjectKinds> _validMemberKindOf = new();
    private BitArray<ObjectKinds> _invalidMemberKindOf = new();

    internal void SetMemberKinds(BitArray<ObjectKinds> validMemberKindOf, BitArray<ObjectKinds> invalidMemberKindOf)
    {
        _validMemberKindOf = validMemberKindOf;
        _invalidMemberKindOf = invalidMemberKindOf;
    }

    internal void SetActiveBattlePlan(BattlePlanType battlePlan, float armorDamageScalar, float sightRangeScalar)
    {
        _bombardmentActive = battlePlan is BattlePlanType.Bombardment;
        _holdTheLineActive = battlePlan is BattlePlanType.HoldTheLine;
        _searchAndDestroyActive = battlePlan is BattlePlanType.SearchAndDestroy;
        _activeArmorDamageScalar = armorDamageScalar;
        _activeSightRangeScalar = sightRangeScalar;
    }

    internal void ClearBattlePlan()
    {
        _bombardmentActive = false;
        _holdTheLineActive = false;
        _searchAndDestroyActive = false;
        _activeArmorDamageScalar = 1;
        _activeSightRangeScalar = 1;
    }

    public void Persist(StatePersister persister)
    {

        persister.PersistSingle(ref _activeArmorDamageScalar);
        persister.PersistSingle(ref _activeSightRangeScalar);

        // yes, this is actually parsed in a different order from BattlePlanUpdate
        persister.PersistBoolean(ref _bombardmentActive);
        persister.SkipUnknownBytes(3);
        persister.PersistBoolean(ref _holdTheLineActive);
        persister.SkipUnknownBytes(3);
        persister.PersistBoolean(ref _searchAndDestroyActive);
        persister.SkipUnknownBytes(3);

        persister.PersistBitArray(ref _validMemberKindOf);
        persister.PersistBitArray(ref _invalidMemberKindOf);
    }
}

public enum PlayerType
{
    Human,
    Computer,
}

public sealed class ResourceGatheringManager : IPersistableObject
{
    // TODO(Port) all of this

    private readonly List<ObjectId> _supplyWarehouses = [];
    private readonly List<ObjectId> _supplyCenters = [];

    public void Persist(StatePersister persister)
    {
        persister.PersistVersion(1);

        persister.PersistList(
            _supplyWarehouses,
            static (StatePersister persister, ref ObjectId item) =>
            {
                persister.PersistObjectIdValue(ref item);
            });

        persister.PersistList(
            _supplyCenters,
            static (StatePersister persister, ref ObjectId item) =>
            {
                persister.PersistObjectIdValue(ref item);
            });
    }
}

public sealed class Handicap
{
    // HandicapType.Count * ThingType.Count matrix of handicaps
    private readonly float[,] _handicaps;

    public Handicap()
    {
        _handicaps = new float[(int)HandicapType.Count, (int)ThingType.Count];
        Init();
    }

    public float GetHandicap(HandicapType t, ObjectDefinition thingTemplate)
    {
        var thingType = GetBestThingType(thingTemplate);
        return _handicaps[(int)t, (int)thingType];
    }

    public void ReadFromDict(AssetPropertyCollection props)
    {
        Span<string> htNames = [
            "BUILDCOST",
            "BUILDTIME"
        ];

        Span<string> ttNames = [
            "GENERIC",
            "BUILDINGS"
        ];

        for (var i = 0; i < htNames.Length; i++)
        {
            for (var j = 0; j < ttNames.Length; j++)
            {
                var name = $"HANDICAP_{htNames[i]}_{ttNames[j]}";
                if (props.TryGetValue(name, out var value) && value.GetReal() is float handicap)
                {
                    _handicaps[i, j] = handicap;
                }
            }
        }
    }

    private static ThingType GetBestThingType(ObjectDefinition thingTemplate)
    {
        if (thingTemplate.IsKindOf(ObjectKinds.Structure))
        {
            return ThingType.Buildings;
        }
        return ThingType.Generic;
    }

    public void Init()
    {
        for (var i = 0; i < _handicaps.GetLength(0); i++)
        {
            for (var j = 0; j < _handicaps.GetLength(1); j++)
            {
                _handicaps[i, j] = 1.0f;
            }
        }
    }

    public enum HandicapType
    {
        BuildCost = 0,
        BuildTime,
        Count,
    }

    public enum ThingType
    {
        Generic = 0,
        Buildings,
        Count,
    }
}


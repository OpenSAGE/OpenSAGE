using System;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Gui.ControlBar
{
    public sealed class CommandButton : BaseAsset
    {
        public CommandButton(){ }

        public CommandButton(CommandType commandType, LazyAssetReference<ObjectDefinition> definition)
        {
            Command = commandType;
            Object = definition;
            ButtonImage = definition.Value.ButtonImage;
        }

        internal static CommandButton Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                 (x, name) => x.SetNameAndInstanceId("CommandButton", name),
                 FieldParseTable);
        }

        private static readonly IniParseTable<CommandButton> FieldParseTable = new IniParseTable<CommandButton>
        {
            { "Command", (parser, x) => x.Command = parser.ParseEnum<CommandType>() },
            { "Options", (parser, x) => x.Options = parser.ParseEnumBitArray<CommandButtonOption>() },
            { "CursorName", (parser, x) => x.CursorName = parser.ParseAssetReference() },
            { "InvalidCursorName", (parser, x) => x.InvalidCursorName = parser.ParseAssetReference() },
            { "SpecialPower", (parser, x) => x.SpecialPower = parser.ParseSpecialPowerReference() },
            { "TextLabel", (parser, x) => x.TextLabel = parser.ParseLocalizedStringKey() },
            { "ConflictingLabel", (parser, x) => x.ConflictingLabel = parser.ParseLocalizedStringKey() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseMappedImageReference() },
            { "ButtonBorderType", (parser, x) => x.ButtonBorderType = parser.ParseEnum<CommandButtonBorderType>() },
            { "DescriptLabel", (parser, x) => x.DescriptLabel = parser.ParseLocalizedStringKey() },
            { "MaxShotsToFire", (parser, x) => x.MaxShotsToFire = parser.ParseInteger() },
            { "Object", (parser, x) => x.Object = parser.ParseObjectReference() },
            { "RadiusCursorType", (parser, x) => x.RadiusCursorType = parser.ParseAssetReference() },
            { "Science", (parser, x) => x.Science = parser.ParseAssetReferenceArray() },
            { "WeaponSlot", (parser, x) => x.WeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "UnitSpecificSound", (parser, x) => x.UnitSpecificSound = parser.ParseAssetReference() },
            { "UnitSpecificSound2", (parser, x) => x.UnitSpecificSound2 = parser.ParseAssetReference() },
            { "Upgrade", (parser, x) => x.Upgrade = parser.ParseUpgradeReference() },
            { "InPalantir", (parser, x) => x.InPalantir = parser.ParseBoolean() },
            { "DoubleClick", (parser, x) => x.DoubleClick = parser.ParseBoolean() },
            { "AutoAbility", (parser, x) => x.AutoAbility = parser.ParseBoolean() },
            { "SetAutoAbilityUnitSound", (parser, x) => x.SetAutoAbilityUnitSound = parser.ParseAssetReference() },
            { "AutoDelay", (parser, x) => x.AutoDelay = parser.ParseFloat() },
            { "AffectsAllies", (parser, x) => x.AffectsAllies = parser.ParseBoolean() },
            { "NeedDamagedTarget", (parser, x) => x.NeedDamagedTarget = parser.ParseBoolean() },
            { "PresetRange", (parser, x) => x.PresetRange = parser.ParseFloat() },
            { "FlagsUsedForToggle", (parser, x) => x.FlagsUsedForToggle = parser.ParseEnum<ModelConditionFlag>() },
            { "DisableOnModelCondition", (parser, x) => x.DisableOnModelCondition = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "EnableOnModelCondition", (parser, x) => x.EnableOnModelCondition = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "CommandTrigger", (parser, x) => x.CommandTrigger = parser.ParseAssetReference() },
            { "WeaponSlotToggle1", (parser, x) => x.WeaponSlotToggle1 = parser.ParseEnum<WeaponSlot>() },
            { "WeaponSlotToggle2", (parser, x) => x.WeaponSlotToggle2 = parser.ParseEnum<WeaponSlot>() },
            { "NeededUpgrade", (parser, x) => x.NeededUpgrade = parser.ParseAssetReference() },
            { "BuildUpgrades", (parser, x) => x.BuildUpgrades = parser.ParseAssetReference() },
            { "Radial", (parser, x) => x.Radial = parser.ParseBoolean() },
            { "IsClickable", (parser, x) => x.IsClickable = parser.ParseBoolean() },
            { "ShowProductionCount", (parser, x) => x.ShowProductionCount = parser.ParseBoolean() },
            { "AffectsKindOf", (parser, x) => x.AffectsKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "RequiresValidContainer", (parser, x) => x.RequiresValidContainer = parser.ParseBoolean() },
            { "LacksPrerequisiteLabel", (parser, x) => x.LacksPrerequisiteLabel = parser.ParseLocalizedStringKey() },
            { "Stances", (parser, x) => x.Stances = parser.ParseAssetReferenceArray() },
            { "TriggerWhenReady", (parser, x) => x.TriggerWhenReady = parser.ParseBoolean() },
            { "AutoAbilityDisallowedOnModelCondition", (parser, x) => x.AutoAbilityDisallowedOnModelCondition = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "ShowButton", (parser, x) => x.ShowButton = parser.ParseBoolean() },
            { "CommandRangeStart", (parser, x) => x.CommandRangeStart = parser.ParseInteger() },
            { "CommandRangeCount", (parser, x) => x.CommandRangeCount = parser.ParseInteger() },
            { "PurchasedLabel", (parser, x) => x.PurchasedLabel = parser.ParseLocalizedStringKey() },
            { "NeededUpgradeAny", (parser, x) => x.NeededUpgradeAny = parser.ParseBoolean() },
            { "CreateAHeroUIAllowableUpgrades", (parser, x) => x.CreateAHeroUIAllowableUpgrades = parser.ParseAssetReferenceArray() },
            { "CreateAHeroUIMinimumLevel", (parser, x) => x.CreateAHeroUIMinimumLevel = parser.ParseInteger() },
            { "CreateAHeroUIPrerequisiteButtonName", (parser, x) => x.CreateAHeroUIPrerequisiteButtonName = parser.ParseString() },
            { "ToggleButtonName", (parser, x) => x.ToggleButtonName = parser.ParseAssetReference() },
            { "CreateAHeroUICostIfSelected", (parser, x) => x.CreateAHeroUICostIfSelected = parser.ParseInteger() }
        };

        public CommandType Command { get; private set; }
        public LazyAssetReference<SpecialPower> SpecialPower { get; private set; }
        public LazyAssetReference<UpgradeTemplate> Upgrade { get; private set; }
        public string[] Science { get; private set; }
        public BitArray<CommandButtonOption> Options { get; private set; }
        public string TextLabel { get; private set; }
        
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string ConflictingLabel { get; private set; }

        public LazyAssetReference<MappedImage> ButtonImage { get; private set; }
        public CommandButtonBorderType ButtonBorderType { get; private set; }
        public string DescriptLabel { get; private set; }
        public string RadiusCursorType { get; private set; }
        public string CursorName { get; private set; }
        public string InvalidCursorName { get; private set; }

        public WeaponSlot WeaponSlot { get; private set; }
        public string UnitSpecificSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string UnitSpecificSound2 { get; private set; }

        public int MaxShotsToFire { get; private set; }
        public LazyAssetReference<ObjectDefinition> Object { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool InPalantir { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool DoubleClick { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ModelConditionFlag? FlagsUsedForToggle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AutoAbility { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SetAutoAbilityUnitSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float AutoDelay { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AffectsAllies { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool NeedDamagedTarget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float PresetRange { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ModelConditionFlag> DisableOnModelCondition { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ModelConditionFlag> EnableOnModelCondition { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string CommandTrigger { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public WeaponSlot? WeaponSlotToggle1 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public WeaponSlot? WeaponSlotToggle2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string NeededUpgrade { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string BuildUpgrades { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool Radial { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IsClickable { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShowProductionCount { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ObjectKinds> AffectsKindOf { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RequiresValidContainer { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string LacksPrerequisiteLabel { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] Stances { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool TriggerWhenReady { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> AutoAbilityDisallowedOnModelCondition { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShowButton { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int CommandRangeStart { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int CommandRangeCount { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string PurchasedLabel { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool NeededUpgradeAny { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] CreateAHeroUIAllowableUpgrades { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int CreateAHeroUIMinimumLevel { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string CreateAHeroUIPrerequisiteButtonName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ToggleButtonName { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int CreateAHeroUICostIfSelected { get; private set; }
    }

    public enum CommandType
    {
        [IniEnum("NONE"), AddedIn(SageGame.Bfme2)]
        None,

        [IniEnum("PLACE_BEACON")]
        PlaceBeacon,

        [IniEnum("SPECIAL_POWER")]
        SpecialPower,

        [IniEnum("SPECIAL_POWER_FROM_COMMAND_CENTER")]
        SpecialPowerFromCommandCenter,

        [IniEnum("SPECIAL_POWER_FROM_SHORTCUT"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SpecialPowerFromShortcut,

        [IniEnum("OBJECT_UPGRADE")]
        ObjectUpgrade,

        [IniEnum("PLAYER_UPGRADE")]
        PlayerUpgrade,

        [IniEnum("EXIT_CONTAINER")]
        ExitContainer,

        [IniEnum("EVACUATE")]
        Evacuate,

        [IniEnum("EXECUTE_RAILED_TRANSPORT")]
        ExecuteRailedTransport,

        [IniEnum("COMBATDROP")]
        CombatDrop,

        [IniEnum("GUARD")]
        Guard,

        [IniEnum("GUARD_WITHOUT_PURSUIT")]
        GuardWithoutPursuit,

        [IniEnum("GUARD_FLYING_UNITS_ONLY")]
        GuardFlyingUnitsOnly,

        [IniEnum("ATTACK_MOVE")]
        AttackMove,

        [IniEnum("STOP")]
        Stop,

        [IniEnum("FIRE_WEAPON")]
        FireWeapon,

        [IniEnum("SWITCH_WEAPON")]
        SwitchWeapon,

        [IniEnum("DOZER_CONSTRUCT_CANCEL")]
        DozerConstructCancel,

        [IniEnum("DOZER_CONSTRUCT")]
        DozerConstruct,

        [IniEnum("CANCEL_UNIT_BUILD")]
        CancelUnitBuild,

        [IniEnum("UNIT_BUILD")]
        UnitBuild,

        [IniEnum("PURCHASE_SCIENCE")]
        PurchaseScience,

        [IniEnum("TOGGLE_OVERCHARGE")]
        ToggleOvercharge,

        [IniEnum("SET_RALLY_POINT")]
        SetRallyPoint,

        [IniEnum("SELL")]
        Sell,

        [IniEnum("CANCEL_UPGRADE")]
        CancelUpgrade,

        [IniEnum("CONVERT_TO_CARBOMB")]
        ConvertToCarBomb,

        [IniEnum("HIJACK_VEHICLE")]
        HijackVehicle,

        [IniEnum("HACK_INTERNET")]
        HackInternet,

        [IniEnum("SABOTAGE_BUILDING"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SabotageBuilding,

        [IniEnum("SELECT_ALL_UNITS_OF_TYPE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SelectAllUnitsOfType,

        [IniEnum("SPECIAL_POWER_CONSTRUCT"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SpecialPowerConstruct,

        [IniEnum("SPECIAL_POWER_CONSTRUCT_FROM_SHORTCUT"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SpecialPowerConstructFromShortcut,

        [IniEnum("HORDE_TOGGLE_FORMATION"), AddedIn(SageGame.Bfme)]
        HordeToggleFormation,

        [IniEnum("TOGGLE_WEAPONSET"), AddedIn(SageGame.Bfme)]
        ToggleWeaponSet,

        [IniEnum("WAKE_AUTO_PICKUP"), AddedIn(SageGame.Bfme)]
        WakeAutoPickup,

        [IniEnum("BLOODTHIRSTY"), AddedIn(SageGame.Bfme)]
        Bloodthirsty,

        [IniEnum("MONSTERDOCK"), AddedIn(SageGame.Bfme)]
        MonsterDock,

        [IniEnum("CREW_EVACUATE"), AddedIn(SageGame.Bfme)]
        CrewEvacuate,

        [IniEnum("TOGGLE_WEAPON"), AddedIn(SageGame.Bfme)]
        ToggleWeapon,

        [IniEnum("EVACUATE_CONTESTED"), AddedIn(SageGame.Bfme)]
        EvacuateContested,

        [IniEnum("FOUNDATION_CONSTRUCT"), AddedIn(SageGame.Bfme)]
        FoundationConstruct,

        [IniEnum("CASTLE_UNPACK"), AddedIn(SageGame.Bfme)]
        CastleUnpack,

        [IniEnum("CASTLE_UNPACK_EXPLICIT_OBJECT"), AddedIn(SageGame.Bfme)]
        CastleUnpackExplicitObject,

        [IniEnum("CASTLE_UPGRADE"), AddedIn(SageGame.Bfme)]
        CastleUpgrade,

        [IniEnum("REVIVE"), AddedIn(SageGame.Bfme)]
        Revive,

        [IniEnum("FOUNDATION_CONSTRUCT_CANCEL"), AddedIn(SageGame.Bfme)]
        FoundationConstructCancel,

        [IniEnum("SPELL_BOOK"), AddedIn(SageGame.Bfme)]
        SpellBook,

        [IniEnum("ONE_RING"), AddedIn(SageGame.Bfme)]
        OneRing,

        [IniEnum("TOGGLE_NO_AUTO_ACQUIRE"), AddedIn(SageGame.Bfme)]
        ToggleNoAutoAcquire,

        [IniEnum("TOGGLE_GATE"), AddedIn(SageGame.Bfme)]
        ToggleGate,

        [IniEnum("START_SELF_REPAIR"), AddedIn(SageGame.Bfme)]
        StartSelfRepair,

        [IniEnum("POP_VISIBLE_COMMAND_RANGE"), AddedIn(SageGame.Bfme2)]
        PopVisibleCommandRange,

        [IniEnum("TOGGLE_STANCE"), AddedIn(SageGame.Bfme2)]
        ToggleStance,

        [IniEnum("SET_STANCE"), AddedIn(SageGame.Bfme2)]
        SetStance,

        [IniEnum("PUSH_VISIBLE_COMMAND_RANGE"), AddedIn(SageGame.Bfme2)]
        PushVisibleCommandRange,

        [IniEnum("START_NEIGHBORHOOD_REPAIR"), AddedIn(SageGame.Bfme2)]
        StartNeighborhoodRepair,

        [IniEnum("CANCEL_NEIGHBORHOOD"), AddedIn(SageGame.Bfme2)]
        CancelNeighborhood,

        [IniEnum("SPECIAL_POWER_TOGGLE"), AddedIn(SageGame.Bfme2)]
        SpecialPowerToggle,
    }

    public enum CommandButtonBorderType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("ACTION")]
        Action,

        [IniEnum("BUILD")]
        Build,

        [IniEnum("UPGRADE")]
        Upgrade,

        [IniEnum("SYSTEM")]
        System
    }

    [Flags]
    public enum CommandButtonOption
    {
        [IniEnum("OK_FOR_MULTI_SELECT")]
        OkForMultiSelect,

        [IniEnum("CHECK_LIKE")]
        CheckLike,

        [IniEnum("NEED_TARGET_ENEMY_OBJECT")]
        NeedTargetEnemyObject,

        [IniEnum("NEED_TARGET_NEUTRAL_OBJECT")]
        NeedTargetNeutralObject,

        [IniEnum("NEED_TARGET_ALLY_OBJECT")]
        NeedTargetAllyObject,

        [IniEnum("CONTEXTMODE_COMMAND")]
        ContextModeCommand,

        [IniEnum("OPTION_ONE")]
        OptionOne,

        [IniEnum("OPTION_TWO")]
        OptionTwo,

        [IniEnum("OPTION_THREE")]
        OptionThree,

        [IniEnum("NEED_TARGET_POS")]
        NeedTargetPos,

        [IniEnum("NOT_QUEUEABLE")]
        NotQueueable,

        [IniEnum("IGNORES_UNDERPOWERED")]
        IgnoresUnderpowered,

        [IniEnum("NEED_SPECIAL_POWER_SCIENCE")]
        NeedSpecialPowerScience,

        [IniEnum("SCRIPT_ONLY")]
        ScriptOnly,

        [IniEnum("NEED_UPGRADE")]
        NeedUpgrade,

        [IniEnum("USES_MINE_CLEARING_WEAPONSET")]
        UsesMineClearingWeaponSet,

        [IniEnum("CAN_USE_WAYPOINTS"), AddedIn(SageGame.CncGeneralsZeroHour)]
        CanUseWaypoints,

        [IniEnum("MUST_BE_STOPPED"), AddedIn(SageGame.CncGeneralsZeroHour)]
        MustBeStopped,

        [IniEnum("TOGGLE_IMAGE_ON_FORMATION"), AddedIn(SageGame.Bfme)]
        ToggleImageOnFormation,

        [IniEnum("TOGGLE_IMAGE_ON_WEAPONSET"), AddedIn(SageGame.Bfme)]
        ToggleImageOnWeaponSet,

        [IniEnum("ALLOW_SHRUBBERY_TARGET"), AddedIn(SageGame.Bfme)]
        AllowShrubberyTarget,

        [IniEnum("ALLOW_ROCK_TARGET"), AddedIn(SageGame.Bfme)]
        AllowRockTarget,

        [IniEnum("NONPRESSABLE"), AddedIn(SageGame.Bfme)]
        NonPressable,

        [IniEnum("CANCELABLE"), AddedIn(SageGame.Bfme)]
        Cancelable,

        [IniEnum("UNMOUNTED_ONLY"), AddedIn(SageGame.Bfme)]
        UnmountedOnly,

        [IniEnum("MOUNTED_ONLY"), AddedIn(SageGame.Bfme)]
        MountedOnly,

        [IniEnum("ON_GROUND_ONLY"), AddedIn(SageGame.Bfme)]
        OnGroundOnly,

        [IniEnum("HIDE_WHILE_DISABLED"), AddedIn(SageGame.Bfme)]
        HideWhileDisabled,

        [IniEnum("NO_PLAY_UNIT_SPECIFIC_SOUND_FOR_AUTO_ABILITY"), AddedIn(SageGame.Bfme)]
        NoPlayUnitSpecificSoundForAutoAbility,

        [IniEnum("TOGGLE_IMAGE_ON_WEAPON"), AddedIn(SageGame.Bfme)]
        ToggleImageOnWeapon,

        [IniEnum("NEEDS_CASTLE_KINDOF"), AddedIn(SageGame.Bfme)]
        NeedsCastleKindOf,

        [IniEnum("OK_FOR_MULTI_EXECUTE"), AddedIn(SageGame.Bfme2)]
        OkForMultiExecute,
    }

    //only used in Generals, ZH and BFME, since BFME2 those types are replaced by named blocks
    public enum CommandButtonRadiusCursorType
    {
        [IniEnum("NONE"), AddedIn(SageGame.Bfme)]
        None,

        [IniEnum("DAISYCUTTER")]
        DaisyCutter,

        [IniEnum("NAPALMSTRIKE")]
        NapalmStrike,

        [IniEnum("PARADROP")]
        Paradrop,

        [IniEnum("CLUSTERMINES")]
        ClusterMines,

        [IniEnum("EMPPULSE")]
        EmpPulse,

        [IniEnum("A10STRIKE")]
        A10Strike,

        [IniEnum("PARTICLECANNON")]
        ParticleCannon,

        [IniEnum("CARPETBOMB")]
        CarpetBomb,

        [IniEnum("NUCLEARMISSILE")]
        NuclearMissile,

        [IniEnum("SCUDSTORM")]
        ScudStorm,

        [IniEnum("ARTILLERYBARRAGE")]
        ArtilleryBarrage,

        [IniEnum("SPYSATELLITE")]
        SpySatellite,

        [IniEnum("SPYDRONE")]
        SpyDrone,

        [IniEnum("RADAR")]
        Radar,

        [IniEnum("AMBUSH")]
        Ambush,

        [IniEnum("EMERGENCY_REPAIR")]
        EmergencyRepair,

        [IniEnum("ANTHRAXBOMB")]
        AnthraxBomb,

        [IniEnum("GUARD_AREA")]
        GuardArea,

        [IniEnum("ATTACK_CONTINUE_AREA")]
        AttackContinueArea,

        [IniEnum("ATTACK_SCATTER_AREA")]
        AttackScatterArea,

        [IniEnum("ATTACK_DAMAGE_AREA")]
        AttackDamageArea,

        [IniEnum("SUPERWEAPON_SCATTER_AREA")]
        SuperweaponScatterArea,

        [IniEnum("FRIENDLY_SPECIALPOWER")]
        FriendlySpecialPower,

        [IniEnum("OFFENSIVE_SPECIALPOWER")]
        OffensiveSpecialPower,

        [IniEnum("SPECTREGUNSHIP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SpectreGunship,

        [IniEnum("FRENZY"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Frenzy,

        [IniEnum("CLEARMINES"), AddedIn(SageGame.CncGeneralsZeroHour)]
        ClearMines,

        [IniEnum("AMBULANCE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Ambulance,

        [IniEnum("HELIX_NAPALM_BOMB"), AddedIn(SageGame.CncGeneralsZeroHour)]
        HelixNapalmBomb,

        [IniEnum("FIRE_BREATH"), AddedIn(SageGame.Bfme)]
        FireBreath,

        [IniEnum("TRAINING"), AddedIn(SageGame.Bfme)]
        Training,

        [IniEnum("SUMMON_OATH_BREAKERS"), AddedIn(SageGame.Bfme)]
        SummonOathBreakers,

        [IniEnum("KINGS_FAVOR"), AddedIn(SageGame.Bfme)]
        KingsFavor,

        [IniEnum("CAPTAIN_OF_GONDOR"), AddedIn(SageGame.Bfme)]
        CaptainOfGondor,

        [IniEnum("ARROWSTORM"), AddedIn(SageGame.Bfme)]
        ArrowStorm,

        [IniEnum("ATHELAS"), AddedIn(SageGame.Bfme)]
        Athelas,

        [IniEnum("ARCHERY_TRAINING"), AddedIn(SageGame.Bfme)]
        ArcheryTraining,

        [IniEnum("LIGHTNING_SWORD"), AddedIn(SageGame.Bfme)]
        LightningSword,

        [IniEnum("LEAP"), AddedIn(SageGame.Bfme)]
        Leap,

        [IniEnum("FELL_BEAST_SWOOP"), AddedIn(SageGame.Bfme)]
        FellBeastSwoop,

        [IniEnum("EAGLE_SWOOP"), AddedIn(SageGame.Bfme)]
        EagleSwoop,

        [IniEnum("REVEAL_MAP_AREA"), AddedIn(SageGame.Bfme)]
        RevealMapArea,

        [IniEnum("WAR_CHANT"), AddedIn(SageGame.Bfme)]
        WarChant,

        [IniEnum("INDUSTRY"), AddedIn(SageGame.Bfme)]
        Industry,

        [IniEnum("DEVASTATION"), AddedIn(SageGame.Bfme)]
        Devastation,

        [IniEnum("TAINT"), AddedIn(SageGame.Bfme)]
        Taint,

        [IniEnum("EYE_OF_SAURON"), AddedIn(SageGame.Bfme)]
        EyeOfSauron,

        [IniEnum("SUMMON_BALROG"), AddedIn(SageGame.Bfme)]
        SummonBalrog,

        [IniEnum("HEAL"), AddedIn(SageGame.Bfme)]
        Heal,

        [IniEnum("ELVEN_ALLIES"), AddedIn(SageGame.Bfme)]
        ElvenAllies,

        [IniEnum("ROHAN_ALLIES"), AddedIn(SageGame.Bfme)]
        RohanAllies,

        [IniEnum("ELVEN_WOOD"), AddedIn(SageGame.Bfme)]
        ElvenWood,

        [IniEnum("ENT_ALLIES"), AddedIn(SageGame.Bfme)]
        EntAllies,

        [IniEnum("ARMY_OF_THE_DEAD"), AddedIn(SageGame.Bfme)]
        ArmyOfTheDead,

        [IniEnum("EAGLE_ALLIES"), AddedIn(SageGame.Bfme)]
        EagleAllies,

        [IniEnum("PALANTIR_VISION"), AddedIn(SageGame.Bfme)]
        PalantirVision,

        [IniEnum("SPEECH_CRAFT"), AddedIn(SageGame.Bfme)]
        SpeechCraft,

        [IniEnum("DOMINATE"), AddedIn(SageGame.Bfme)]
        Dominate,
    }
}

using System;
using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;

namespace OpenSage.Data.Ini
{
    public sealed class CommandButton
    {
        internal static CommandButton Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                 (x, name) => x.Name = name,
                 FieldParseTable);
        }

        private static readonly IniParseTable<CommandButton> FieldParseTable = new IniParseTable<CommandButton>
        {
            { "Command", (parser, x) => x.Command = parser.ParseEnum<CommandType>() },
            { "Options", (parser, x) => x.Options = parser.ParseEnumBitArray<CommandButtonOption>() },
            { "CursorName", (parser, x) => x.CursorName = parser.ParseAssetReference() },
            { "InvalidCursorName", (parser, x) => x.InvalidCursorName = parser.ParseAssetReference() },
            { "SpecialPower", (parser, x) => x.SpecialPower = parser.ParseAssetReference() },
            { "TextLabel", (parser, x) => x.TextLabel = parser.ParseLocalizedStringKey() },
            { "ConflictingLabel", (parser, x) => x.ConflictingLabel = parser.ParseLocalizedStringKey() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseAssetReference() },
            { "ButtonBorderType", (parser, x) => x.ButtonBorderType = parser.ParseEnum<CommandButtonBorderType>() },
            { "DescriptLabel", (parser, x) => x.DescriptLabel = parser.ParseLocalizedStringKey() },
            { "MaxShotsToFire", (parser, x) => x.MaxShotsToFire = parser.ParseInteger() },
            { "Object", (parser, x) => x.Object = parser.ParseAssetReference() },
            { "RadiusCursorType", (parser, x) => x.RadiusCursorType = parser.ParseEnum<CommandButtonRadiusCursorType>() },
            { "Science", (parser, x) => x.Science = parser.ParseAssetReferenceArray() },
            { "WeaponSlot", (parser, x) => x.WeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "UnitSpecificSound", (parser, x) => x.UnitSpecificSound = parser.ParseAssetReference() },
            { "UnitSpecificSound2", (parser, x) => x.UnitSpecificSound2 = parser.ParseAssetReference() },
            { "Upgrade", (parser, x) => x.Upgrade = parser.ParseAssetReference() },
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
        };

        public string Name { get; private set; }

        public CommandType Command { get; private set; }
        public string SpecialPower { get; private set; }
        public string Upgrade { get; private set; }
        public string[] Science { get; private set; }
        public BitArray<CommandButtonOption> Options { get; private set; }
        public string TextLabel { get; private set; }
        
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string ConflictingLabel { get; private set; }

        public string ButtonImage { get; private set; }
        public CommandButtonBorderType ButtonBorderType { get; private set; }
        public string DescriptLabel { get; private set; }
        public CommandButtonRadiusCursorType RadiusCursorType { get; private set; }
        public string CursorName { get; private set; }
        public string InvalidCursorName { get; private set; }

        public WeaponSlot WeaponSlot { get; private set; }
        public string UnitSpecificSound { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string UnitSpecificSound2 { get; private set; }

        public int MaxShotsToFire { get; private set; }
        public string Object { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool InPalantir { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool DoubleClick { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public ModelConditionFlag? FlagsUsedForToggle { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool AutoAbility { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string SetAutoAbilityUnitSound { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float AutoDelay { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool AffectsAllies { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool NeedDamagedTarget { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float PresetRange { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public BitArray<ModelConditionFlag> DisableOnModelCondition { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public BitArray<ModelConditionFlag> EnableOnModelCondition { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string CommandTrigger { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public WeaponSlot? WeaponSlotToggle1 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public WeaponSlot? WeaponSlotToggle2 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string NeededUpgrade { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string BuildUpgrades { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool Radial { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool IsClickable { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool ShowProductionCount { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public BitArray<ObjectKinds> AffectsKindOf { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool RequiresValidContainer { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string LacksPrerequisiteLabel { get; private set; }
    }

    public enum CommandType
    {
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

        [IniEnum("HORDE_TOGGLE_FORMATION"), AddedIn(SageGame.BattleForMiddleEarth)]
        HordeToggleFormation,

        [IniEnum("TOGGLE_WEAPONSET"), AddedIn(SageGame.BattleForMiddleEarth)]
        ToggleWeaponSet,

        [IniEnum("WAKE_AUTO_PICKUP"), AddedIn(SageGame.BattleForMiddleEarth)]
        WakeAutoPickup,

        [IniEnum("BLOODTHIRSTY"), AddedIn(SageGame.BattleForMiddleEarth)]
        Bloodthirsty,

        [IniEnum("MONSTERDOCK"), AddedIn(SageGame.BattleForMiddleEarth)]
        MonsterDock,

        [IniEnum("CREW_EVACUATE"), AddedIn(SageGame.BattleForMiddleEarth)]
        CrewEvacuate,

        [IniEnum("TOGGLE_WEAPON"), AddedIn(SageGame.BattleForMiddleEarth)]
        ToggleWeapon,

        [IniEnum("EVACUATE_CONTESTED"), AddedIn(SageGame.BattleForMiddleEarth)]
        EvacuateContested,

        [IniEnum("FOUNDATION_CONSTRUCT"), AddedIn(SageGame.BattleForMiddleEarth)]
        FoundationConstruct,

        [IniEnum("CASTLE_UNPACK"), AddedIn(SageGame.BattleForMiddleEarth)]
        CastleUnpack,

        [IniEnum("CASTLE_UNPACK_EXPLICIT_OBJECT"), AddedIn(SageGame.BattleForMiddleEarth)]
        CastleUnpackExplicitObject,

        [IniEnum("CASTLE_UPGRADE"), AddedIn(SageGame.BattleForMiddleEarth)]
        CastleUpgrade,

        [IniEnum("REVIVE"), AddedIn(SageGame.BattleForMiddleEarth)]
        Revive,

        [IniEnum("FOUNDATION_CONSTRUCT_CANCEL"), AddedIn(SageGame.BattleForMiddleEarth)]
        FoundationConstructCancel,

        [IniEnum("SPELL_BOOK"), AddedIn(SageGame.BattleForMiddleEarth)]
        SpellBook,

        [IniEnum("ONE_RING"), AddedIn(SageGame.BattleForMiddleEarth)]
        OneRing,

        [IniEnum("TOGGLE_NO_AUTO_ACQUIRE"), AddedIn(SageGame.BattleForMiddleEarth)]
        ToggleNoAutoAcquire,

        [IniEnum("TOGGLE_GATE"), AddedIn(SageGame.BattleForMiddleEarth)]
        ToggleGate,

        [IniEnum("START_SELF_REPAIR"), AddedIn(SageGame.BattleForMiddleEarth)]
        StartSelfRepair,
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

        [IniEnum("TOGGLE_IMAGE_ON_FORMATION"), AddedIn(SageGame.BattleForMiddleEarth)]
        ToggleImageOnFormation,

        [IniEnum("TOGGLE_IMAGE_ON_WEAPONSET"), AddedIn(SageGame.BattleForMiddleEarth)]
        ToggleImageOnWeaponSet,

        [IniEnum("ALLOW_SHRUBBERY_TARGET"), AddedIn(SageGame.BattleForMiddleEarth)]
        AllowShrubberyTarget,

        [IniEnum("ALLOW_ROCK_TARGET"), AddedIn(SageGame.BattleForMiddleEarth)]
        AllowRockTarget,

        [IniEnum("NONPRESSABLE"), AddedIn(SageGame.BattleForMiddleEarth)]
        NonPressable,

        [IniEnum("CANCELABLE"), AddedIn(SageGame.BattleForMiddleEarth)]
        Cancelable,

        [IniEnum("UNMOUNTED_ONLY"), AddedIn(SageGame.BattleForMiddleEarth)]
        UnmountedOnly,

        [IniEnum("MOUNTED_ONLY"), AddedIn(SageGame.BattleForMiddleEarth)]
        MountedOnly,

        [IniEnum("ON_GROUND_ONLY"), AddedIn(SageGame.BattleForMiddleEarth)]
        OnGroundOnly,

        [IniEnum("HIDE_WHILE_DISABLED"), AddedIn(SageGame.BattleForMiddleEarth)]
        HideWhileDisabled,

        [IniEnum("NO_PLAY_UNIT_SPECIFIC_SOUND_FOR_AUTO_ABILITY"), AddedIn(SageGame.BattleForMiddleEarth)]
        NoPlayUnitSpecificSoundForAutoAbility,

        [IniEnum("TOGGLE_IMAGE_ON_WEAPON"), AddedIn(SageGame.BattleForMiddleEarth)]
        ToggleImageOnWeapon,

        [IniEnum("NEEDS_CASTLE_KINDOF"), AddedIn(SageGame.BattleForMiddleEarth)]
        NeedsCastleKindOf,
    }

    public enum WeaponSlot
    {
        [IniEnum("PRIMARY")]
        Primary,

        [IniEnum("SECONDARY")]
        Secondary,

        [IniEnum("TERTIARY")]
        Tertiary
    }

    public enum CommandButtonRadiusCursorType
    {
        [IniEnum("NONE"), AddedIn(SageGame.BattleForMiddleEarth)]
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
        SuperweaponScatterAreaRadiusCursor,

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

        [IniEnum("FIRE_BREATH"), AddedIn(SageGame.BattleForMiddleEarth)]
        FireBreath,

        [IniEnum("TRAINING"), AddedIn(SageGame.BattleForMiddleEarth)]
        Training,

        [IniEnum("SUMMON_OATH_BREAKERS"), AddedIn(SageGame.BattleForMiddleEarth)]
        SummonOathBreakers,

        [IniEnum("KINGS_FAVOR"), AddedIn(SageGame.BattleForMiddleEarth)]
        KingsFavor,

        [IniEnum("CAPTAIN_OF_GONDOR"), AddedIn(SageGame.BattleForMiddleEarth)]
        CaptainOfGondor,

        [IniEnum("ARROWSTORM"), AddedIn(SageGame.BattleForMiddleEarth)]
        ArrowStorm,

        [IniEnum("ATHELAS"), AddedIn(SageGame.BattleForMiddleEarth)]
        Athelas,

        [IniEnum("ARCHERY_TRAINING"), AddedIn(SageGame.BattleForMiddleEarth)]
        ArcheryTraining,

        [IniEnum("LIGHTNING_SWORD"), AddedIn(SageGame.BattleForMiddleEarth)]
        LightningSword,

        [IniEnum("LEAP"), AddedIn(SageGame.BattleForMiddleEarth)]
        Leap,

        [IniEnum("FELL_BEAST_SWOOP"), AddedIn(SageGame.BattleForMiddleEarth)]
        FellBeastSwoop,

        [IniEnum("EAGLE_SWOOP"), AddedIn(SageGame.BattleForMiddleEarth)]
        EagleSwoop,

        [IniEnum("REVEAL_MAP_AREA"), AddedIn(SageGame.BattleForMiddleEarth)]
        RevealMapArea,

        [IniEnum("WAR_CHANT"), AddedIn(SageGame.BattleForMiddleEarth)]
        WarChant,

        [IniEnum("INDUSTRY"), AddedIn(SageGame.BattleForMiddleEarth)]
        Industry,

        [IniEnum("DEVASTATION"), AddedIn(SageGame.BattleForMiddleEarth)]
        Devastation,

        [IniEnum("TAINT"), AddedIn(SageGame.BattleForMiddleEarth)]
        Taint,

        [IniEnum("EYE_OF_SAURON"), AddedIn(SageGame.BattleForMiddleEarth)]
        EyeOfSauron,

        [IniEnum("SUMMON_BALROG"), AddedIn(SageGame.BattleForMiddleEarth)]
        SummonBalrog,

        [IniEnum("HEAL"), AddedIn(SageGame.BattleForMiddleEarth)]
        Heal,

        [IniEnum("ELVEN_ALLIES"), AddedIn(SageGame.BattleForMiddleEarth)]
        ElvenAllies,

        [IniEnum("ROHAN_ALLIES"), AddedIn(SageGame.BattleForMiddleEarth)]
        RohanAllies,

        [IniEnum("ELVEN_WOOD"), AddedIn(SageGame.BattleForMiddleEarth)]
        ElvenWood,

        [IniEnum("ENT_ALLIES"), AddedIn(SageGame.BattleForMiddleEarth)]
        EntAllies,

        [IniEnum("ARMY_OF_THE_DEAD"), AddedIn(SageGame.BattleForMiddleEarth)]
        ArmyOfTheDead,

        [IniEnum("EAGLE_ALLIES"), AddedIn(SageGame.BattleForMiddleEarth)]
        EagleAllies,

        [IniEnum("PALANTIR_VISION"), AddedIn(SageGame.BattleForMiddleEarth)]
        PalantirVision,

        [IniEnum("SPEECH_CRAFT"), AddedIn(SageGame.BattleForMiddleEarth)]
        SpeechCraft,

        [IniEnum("DOMINATE"), AddedIn(SageGame.BattleForMiddleEarth)]
        Dominate,
    }
}

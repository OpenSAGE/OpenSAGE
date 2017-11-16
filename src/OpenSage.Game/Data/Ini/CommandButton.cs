using System;
using OpenSage.Data.Ini.Parser;

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
            {
                "WeaponSlot",
                (parser, x) => x.WeaponSlot = parser.ParseEnum<WeaponSlot>()
            },
            { "UnitSpecificSound", (parser, x) => x.UnitSpecificSound = parser.ParseAssetReference() },
            { "Upgrade", (parser, x) => x.Upgrade = parser.ParseAssetReference() },
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
        public int MaxShotsToFire { get; private set; }
        public string Object { get; private set; }
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
        MustBeStopped
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
    }
}

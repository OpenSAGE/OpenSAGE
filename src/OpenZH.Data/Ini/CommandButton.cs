using System;
using System.Collections.Generic;
using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
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
            { "Options", (parser, x) => x.Options = parser.ParseEnumFlags<CommandButtonOptions>() },
            { "CursorName", (parser, x) => x.CursorName = parser.ParseAsciiString() },
            { "InvalidCursorName", (parser, x) => x.InvalidCursorName = parser.ParseAsciiString() },
            { "SpecialPower", (parser, x) => x.SpecialPower = parser.ParseAsciiString() },
            { "TextLabel", (parser, x) => x.TextLabel = parser.ParseAsciiString() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseAsciiString() },
            { "ButtonBorderType", (parser, x) => x.ButtonBorderType = parser.ParseEnum<CommandButtonBorderType>() },
            { "DescriptLabel", (parser, x) => x.DescriptLabel = parser.ParseAsciiString() },
            { "MaxShotsToFire", (parser, x) => x.MaxShotsToFire = parser.ParseInteger() },
            { "Object", (parser, x) => x.Object = parser.ParseAsciiString() },
            { "RadiusCursorType", (parser, x) => x.RadiusCursorType = parser.ParseEnum<CommandButtonRadiusCursorType>() },
            { "Science", (parser, x) => x.Science = parser.ParseAsciiStringArray() },
            { "WeaponSlot", (parser, x) => x.WeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "UnitSpecificSound", (parser, x) => x.UnitSpecificSound = parser.ParseAsciiString() },
            { "Upgrade", (parser, x) => x.Upgrade = parser.ParseAsciiString() },
        };

        public string Name { get; private set; }

        public CommandType Command { get; private set; }
        public string SpecialPower { get; private set; }
        public string Upgrade { get; private set; }
        public string[] Science { get; private set; }
        public CommandButtonOptions Options { get; private set; }
        public string TextLabel { get; private set; }
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
        HackInternet
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
    public enum CommandButtonOptions
    {
        None = 0,

        [IniEnum("OK_FOR_MULTI_SELECT")]
        OkForMultiSelect          = 0x0001,

        [IniEnum("CHECK_LIKE")]
        CheckLike                 = 0x0002,

        [IniEnum("NEED_TARGET_ENEMY_OBJECT")]
        NeedTargetEnemyObject     = 0x0004,

        [IniEnum("NEED_TARGET_NEUTRAL_OBJECT")]
        NeedTargetNeutralObject   = 0x0008,

        [IniEnum("NEED_TARGET_ALLY_OBJECT")]
        NeedTargetAllyObject      = 0x0010,

        [IniEnum("CONTEXTMODE_COMMAND")]
        ContextModeCommand        = 0x0020,

        [IniEnum("OPTION_ONE")]
        OptionOne                 = 0x0040,

        [IniEnum("OPTION_TWO")]
        OptionTwo                 = 0x0080,

        [IniEnum("OPTION_THREE")]
        OptionThree               = 0x0100,

        [IniEnum("NEED_TARGET_POS")]
        NeedTargetPos             = 0x0200,

        [IniEnum("NOT_QUEUEABLE")]
        NotQueueable              = 0x0400,

        [IniEnum("IGNORES_UNDERPOWERED")]
        IgnoresUnderpowered       = 0x0800,

        [IniEnum("NEED_SPECIAL_POWER_SCIENCE")]
        NeedSpecialPowerScience   = 0x1000,

        [IniEnum("SCRIPT_ONLY")]
        ScriptOnly                = 0x2000,

        [IniEnum("NEED_UPGRADE")]
        NeedUpgrade               = 0x4000,

        [IniEnum("USES_MINE_CLEARING_WEAPONSET")]
        UsesMineClearingWeaponSet = 0x8000
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
        OffensiveSpecialPower
    }
}

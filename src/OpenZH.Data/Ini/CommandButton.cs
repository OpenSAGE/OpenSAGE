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
            { "Command", (parser, x) => x.Command = parser.ParseCommandType() },
            { "Options", (parser, x) => x.Options = parser.ParseCommandButtonOptions() },
            { "CursorName", (parser, x) => x.CursorName = parser.ParseAsciiString() },
            { "InvalidCursorName", (parser, x) => x.InvalidCursorName = parser.ParseAsciiString() },
            { "SpecialPower", (parser, x) => x.SpecialPower = parser.ParseAsciiString() },
            { "TextLabel", (parser, x) => x.TextLabel = parser.ParseAsciiString() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseAsciiString() },
            { "ButtonBorderType", (parser, x) => x.ButtonBorderType = parser.ParseCommandButtonBorderType() },
            { "DescriptLabel", (parser, x) => x.DescriptLabel = parser.ParseAsciiString() },
            { "MaxShotsToFire", (parser, x) => x.MaxShotsToFire = parser.ParseInteger() },
            { "Object", (parser, x) => x.Object = parser.ParseAsciiString() },
            { "RadiusCursorType", (parser, x) => x.RadiusCursorType = parser.ParseCommandButtonRadiusCursorType() },
            { "Science", (parser, x) => x.Science = parser.ParseAsciiStringArray() },
            { "WeaponSlot", (parser, x) => x.WeaponSlot = parser.ParseWeaponSlot() },
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
        PlaceBeacon,
        SpecialPower,
        SpecialPowerFromCommandCenter,
        ObjectUpgrade,
        PlayerUpgrade,
        ExitContainer,
        Evacuate,
        ExecuteRailedTransport,
        CombatDrop,
        Guard,
        GuardWithoutPursuit,
        GuardFlyingUnitsOnly,
        AttackMove,
        Stop,
        FireWeapon,
        SwitchWeapon,
        DozerConstructCancel,
        DozerConstruct,
        CancelUnitBuild,
        UnitBuild,
        PurchaseScience,
        ToggleOvercharge,
        SetRallyPoint,
        Sell,
        CancelUpgrade,
        ConvertToCarBomb,
        HijackVehicle,
        HackInternet
    }

    public enum CommandButtonBorderType
    {
        None,
        Action,
        Build,
        Upgrade,
        System
    }

    [Flags]
    public enum CommandButtonOptions
    {
        None = 0,

        OkForMultiSelect          = 0x0001,
        CheckLike                 = 0x0002,
        NeedTargetEnemyObject     = 0x0004,
        NeedTargetNeutralObject   = 0x0008,
        NeedTargetAllyObject      = 0x0010,
        ContextModeCommand        = 0x0020,
        OptionOne                 = 0x0040,
        OptionTwo                 = 0x0080,
        OptionThree               = 0x0100,
        NeedTargetPos             = 0x0200,
        NotQueueable              = 0x0400,
        IgnoresUnderpowered       = 0x0800,
        NeedSpecialPowerScience   = 0x1000,
        ScriptOnly                = 0x2000,
        NeedUpgrade               = 0x4000,
        UsesMineClearingWeaponSet = 0x8000
    }

    public enum WeaponSlot
    {
        Primary,
        Secondary,
        Tertiary
    }

    public enum CommandButtonRadiusCursorType
    {
        DaisyCutter,
        NapalmStrike,
        Paradrop,
        ClusterMines,
        EmpPulse,
        A10Strike,
        CarpetBomb,
        NuclearMissile,
        ScudStorm,
        ArtilleryBarrage,
        SpySatellite,
        SpyDrone,
        Radar,
        Ambush,
        EmergencyRepair,
        AnthraxBomb,
        GuardArea,
        AttackContinueArea,
        AttackScatterArea,
        FriendlySpecialPower
    }
}

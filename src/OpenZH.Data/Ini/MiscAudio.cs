using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class MiscAudio
    {
        internal static MiscAudio Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<MiscAudio> FieldParseTable = new IniParseTable<MiscAudio>
        {
            { "RadarNotifyUnitUnderAttackSound", (parser, x) => x.RadarNotifyUnitUnderAttackSound = parser.ParseAsciiString() },
            { "RadarNotifyHarvesterUnderAttackSound", (parser, x) => x.RadarNotifyHarvesterUnderAttackSound = parser.ParseAsciiString() },
            { "RadarNotifyStructureUnderAttackSound", (parser, x) => x.RadarNotifyStructureUnderAttackSound = parser.ParseAsciiString() },
            { "RadarNotifyUnderAttackSound", (parser, x) => x.RadarNotifyUnderAttackSound = parser.ParseAsciiString() },
            { "RadarNotifyInfiltrationSound", (parser, x) => x.RadarNotifyInfiltrationSound = parser.ParseAsciiString() },
            { "RadarNotifyOnlineSound", (parser, x) => x.RadarNotifyOnlineSound = parser.ParseAsciiString() },
            { "RadarNotifyOfflineSound", (parser, x) => x.RadarNotifyOfflineSound = parser.ParseAsciiString() },
            { "LockonTickSound", (parser, x) => x.LockonTickSound = parser.ParseAsciiString() },
            { "DefectorTimerTickSound", (parser, x) => x.DefectorTimerTickSound = parser.ParseAsciiString() },
            { "AllCheerSound", (parser, x) => x.AllCheerSound = parser.ParseAsciiString() },
            { "BattleCrySound", (parser, x) => x.BattleCrySound = parser.ParseAsciiString() },
            { "GUIClickSound", (parser, x) => x.GuiClickSound = parser.ParseAsciiString() },
            { "NoCanDoSound", (parser, x) => x.NoCanDoSound = parser.ParseAsciiString() },
            { "StealthDiscoveredSound", (parser, x) => x.StealthDiscoveredSound = parser.ParseAsciiString() },
            { "StealthNeutralizedSound", (parser, x) => x.StealthNeutralizedSound = parser.ParseAsciiString() },
            { "MoneyDepositSound", (parser, x) => x.MoneyDepositSound = parser.ParseAsciiString() },
            { "MoneyWithdrawSound", (parser, x) => x.MoneyWithdrawSound = parser.ParseAsciiString() },
            { "BuildingDisabled", (parser, x) => x.BuildingDisabled = parser.ParseAsciiString() },
            { "BuildingReenabled", (parser, x) => x.BuildingReenabled = parser.ParseAsciiString() },
            { "VehicleDisabled", (parser, x) => x.VehicleDisabled = parser.ParseAsciiString() },
            { "VehicleReenabled", (parser, x) => x.VehicleReenabled = parser.ParseAsciiString() },
            { "SplatterVehiclePilotsBrain", (parser, x) => x.SplatterVehiclePilotsBrain = parser.ParseAsciiString() },
            { "TerroristInCarMoveVoice", (parser, x) => x.TerroristInCarMoveVoice = parser.ParseAsciiString() },
            { "TerroristInCarAttackVoice", (parser, x) => x.TerroristInCarAttackVoice = parser.ParseAsciiString() },
            { "TerroristInCarSelectVoice", (parser, x) => x.TerroristInCarSelectVoice = parser.ParseAsciiString() },
            { "CrateHeal", (parser, x) => x.CrateHeal = parser.ParseAsciiString() },
            { "CrateShroud", (parser, x) => x.CrateShroud = parser.ParseAsciiString() },
            { "CrateSalvage", (parser, x) => x.CrateSalvage = parser.ParseAsciiString() },
            { "CrateFreeUnit", (parser, x) => x.CrateFreeUnit = parser.ParseAsciiString() },
            { "CrateMoney", (parser, x) => x.CrateMoney = parser.ParseAsciiString() },
            { "UnitPromoted", (parser, x) => x.UnitPromoted = parser.ParseAsciiString() },
            { "RepairSparks", (parser, x) => x.RepairSparks = parser.ParseAsciiString() },
            { "AircraftWheelScreech", (parser, x) => x.AircraftWheelScreech = parser.ParseAsciiString() }
        };

        public string RadarNotifyUnitUnderAttackSound { get; private set; }
        public string RadarNotifyHarvesterUnderAttackSound { get; private set; }
        public string RadarNotifyStructureUnderAttackSound { get; private set; }
        public string RadarNotifyUnderAttackSound { get; private set; }
        public string RadarNotifyInfiltrationSound { get; private set; }
        public string RadarNotifyOnlineSound { get; private set; }
        public string RadarNotifyOfflineSound { get; private set; }
        public string LockonTickSound { get; private set; }
        public string DefectorTimerTickSound { get; private set; }
        public string AllCheerSound { get; private set; }
        public string BattleCrySound { get; private set; }
        public string GuiClickSound { get; private set; }
        public string NoCanDoSound { get; private set; }
        public string StealthDiscoveredSound { get; private set; }
        public string StealthNeutralizedSound { get; private set; }
        public string MoneyDepositSound { get; private set; }
        public string MoneyWithdrawSound { get; private set; }
        public string BuildingDisabled { get; private set; }
        public string BuildingReenabled { get; private set; }
        public string VehicleDisabled { get; private set; }
        public string VehicleReenabled { get; private set; }
        public string SplatterVehiclePilotsBrain { get; private set; }
        public string TerroristInCarMoveVoice { get; private set; }
        public string TerroristInCarAttackVoice { get; private set; }
        public string TerroristInCarSelectVoice { get; private set; }
        public string CrateHeal { get; private set; }
        public string CrateShroud { get; private set; }
        public string CrateSalvage { get; private set; }
        public string CrateFreeUnit { get; private set; }
        public string CrateMoney { get; private set; }
        public string UnitPromoted { get; private set; }
        public string RepairSparks { get; private set; }
        public string AircraftWheelScreech { get; private set; }
    }
}

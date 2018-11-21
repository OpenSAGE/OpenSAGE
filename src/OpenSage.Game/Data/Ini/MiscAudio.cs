using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class MiscAudio
    {
        internal static MiscAudio Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<MiscAudio> FieldParseTable = new IniParseTable<MiscAudio>
        {
            { "RadarNotifyUnitUnderAttackSound", (parser, x) => x.RadarNotifyUnitUnderAttackSound = parser.ParseAssetReference() },
            { "RadarNotifyHarvesterUnderAttackSound", (parser, x) => x.RadarNotifyHarvesterUnderAttackSound = parser.ParseAssetReference() },
            { "RadarNotifyStructureUnderAttackSound", (parser, x) => x.RadarNotifyStructureUnderAttackSound = parser.ParseAssetReference() },
            { "RadarNotifyUnderAttackSound", (parser, x) => x.RadarNotifyUnderAttackSound = parser.ParseAssetReference() },
            { "RadarNotifyInfiltrationSound", (parser, x) => x.RadarNotifyInfiltrationSound = parser.ParseAssetReference() },
            { "RadarNotifyOnlineSound", (parser, x) => x.RadarNotifyOnlineSound = parser.ParseAssetReference() },
            { "RadarNotifyOfflineSound", (parser, x) => x.RadarNotifyOfflineSound = parser.ParseAssetReference() },
            { "LockonTickSound", (parser, x) => x.LockonTickSound = parser.ParseAssetReference() },
            { "DefectorTimerTickSound", (parser, x) => x.DefectorTimerTickSound = parser.ParseAssetReference() },
            { "DefectorTimerDingSound", (parser, x) => x.DefectorTimerDingSound = parser.ParseAssetReference() },
            { "AllCheerSound", (parser, x) => x.AllCheerSound = parser.ParseAssetReference() },
            { "BattleCrySound", (parser, x) => x.BattleCrySound = parser.ParseAssetReference() },
            { "GUIClickSound", (parser, x) => x.GuiClickSound = parser.ParseAssetReference() },
            { "NoCanDoSound", (parser, x) => x.NoCanDoSound = parser.ParseAssetReference() },
            { "StealthDiscoveredSound", (parser, x) => x.StealthDiscoveredSound = parser.ParseAssetReference() },
            { "StealthNeutralizedSound", (parser, x) => x.StealthNeutralizedSound = parser.ParseAssetReference() },
            { "MoneyDepositSound", (parser, x) => x.MoneyDepositSound = parser.ParseAssetReference() },
            { "MoneyWithdrawSound", (parser, x) => x.MoneyWithdrawSound = parser.ParseAssetReference() },
            { "BuildingDisabled", (parser, x) => x.BuildingDisabled = parser.ParseAssetReference() },
            { "BuildingReenabled", (parser, x) => x.BuildingReenabled = parser.ParseAssetReference() },
            { "VehicleDisabled", (parser, x) => x.VehicleDisabled = parser.ParseAssetReference() },
            { "VehicleReenabled", (parser, x) => x.VehicleReenabled = parser.ParseAssetReference() },
            { "SplatterVehiclePilotsBrain", (parser, x) => x.SplatterVehiclePilotsBrain = parser.ParseAssetReference() },
            { "TerroristInCarMoveVoice", (parser, x) => x.TerroristInCarMoveVoice = parser.ParseAssetReference() },
            { "TerroristInCarAttackVoice", (parser, x) => x.TerroristInCarAttackVoice = parser.ParseAssetReference() },
            { "TerroristInCarSelectVoice", (parser, x) => x.TerroristInCarSelectVoice = parser.ParseAssetReference() },
            { "CrateHeal", (parser, x) => x.CrateHeal = parser.ParseAssetReference() },
            { "CrateShroud", (parser, x) => x.CrateShroud = parser.ParseAssetReference() },
            { "CrateSalvage", (parser, x) => x.CrateSalvage = parser.ParseAssetReference() },
            { "CrateFreeUnit", (parser, x) => x.CrateFreeUnit = parser.ParseAssetReference() },
            { "CrateMoney", (parser, x) => x.CrateMoney = parser.ParseAssetReference() },
            { "UnitPromoted", (parser, x) => x.UnitPromoted = parser.ParseAssetReference() },
            { "RepairSparks", (parser, x) => x.RepairSparks = parser.ParseAssetReference() },
            { "AircraftWheelScreech", (parser, x) => x.AircraftWheelScreech = parser.ParseAssetReference() },
            { "SabotageShutDownBuilding", (parser, x) => x.SabotageShutDownBuilding = parser.ParseAssetReference() },
            { "SabotageResetTimeBuilding", (parser, x) => x.SabotageResetTimeBuilding = parser.ParseAssetReference() },
            { "EnterCloseCombat", (parser, x) => x.EnterCloseCombat = parser.ParseAssetReference() },
            { "ExitCloseCombat", (parser, x) => x.ExitCloseCombat = parser.ParseAssetReference() },
            { "IncomingChatNotification", (parser, x) => x.IncomingChatNotification = parser.ParseAssetReference() },
            { "EnabledHotKeyPressed", (parser, x) => x.EnabledHotKeyPressed = parser.ParseAssetReference() },
            { "DisabledHotKeyPressed", (parser, x) => x.DisabledHotKeyPressed = parser.ParseAssetReference() },
            { "LowLODShellMusic", (parser, x) => x.LowLODShellMusic = parser.ParseAssetReference() },
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

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string SabotageShutDownBuilding { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string SabotageResetTimeBuilding { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string DefectorTimerDingSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string EnterCloseCombat { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ExitCloseCombat { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string IncomingChatNotification { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string EnabledHotKeyPressed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string DisabledHotKeyPressed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string LowLODShellMusic { get; private set; }
    }
}

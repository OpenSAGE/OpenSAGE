using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class SpecialPower
    {
        internal static SpecialPower Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<SpecialPower> FieldParseTable = new IniParseTable<SpecialPower>
        {
            { "Enum", (parser, x) => x.Type = parser.ParseEnum<SpecialPowerType>() },
            { "ReloadTime", (parser, x) => x.ReloadTime = parser.ParseInteger() },
            { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseAsciiString() },
            { "PublicTimer", (parser, x) => x.PublicTimer = parser.ParseBoolean() },
            { "SharedSyncedTimer", (parser, x) => x.SharedSyncedTimer = parser.ParseBoolean() },
            { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAsciiString() },
            { "InitiateAtLocationSound", (parser, x) => x.InitiateAtLocationSound = parser.ParseAsciiString() },
            { "ViewObjectDuration", (parser, x) => x.ViewObjectDuration = parser.ParseInteger() },
            { "ViewObjectRange", (parser, x) => x.ViewObjectRange = parser.ParseInteger() },
            { "RadiusCursorRadius", (parser, x) => x.RadiusCursorRadius = parser.ParseInteger() }
        };

        public string Name { get; private set; }

        public SpecialPowerType Type { get; private set; }
        public int ReloadTime { get; private set; }
        public string RequiredScience { get; private set; }
        public bool PublicTimer { get; private set; }
        public bool SharedSyncedTimer { get; private set; }
        public string InitiateSound { get; private set; }
        public string InitiateAtLocationSound { get; private set; }
        public int ViewObjectDuration { get; private set; }
        public int ViewObjectRange { get; private set; }
        public int RadiusCursorRadius { get; private set; }
    }

    public enum SpecialPowerType
    {
        [IniEnum("SPECIAL_DAISY_CUTTER")]
        DaisyCutter,

        [IniEnum("SPECIAL_PARADROP_AMERICA")]
        ParadropAmerica,

        [IniEnum("SPECIAL_CARPET_BOMB")]
        CarpetBomb,

        [IniEnum("SPECIAL_CLUSTER_MINES")]
        ClusterMines,

        [IniEnum("SPECIAL_EMP_PULSE")]
        EmpPulse,

        [IniEnum("SPECIAL_CRATE_DROP")]
        CrateDrop,

        [IniEnum("SPECIAL_A10_THUNDERBOLT_STRIKE")]
        A10ThunderboltStrike,

        [IniEnum("SPECIAL_NAPALM_STRIKE")]
        NapalmStrike,

        [IniEnum("SPECIAL_NEUTRON_MISSILE")]
        NeutronMissile,

        [IniEnum("SPECIAL_DETONATE_DIRTY_NUKE")]
        DetonateDirtyNuke,

        [IniEnum("SPECIAL_SCUD_STORM")]
        ScudStorm,

        [IniEnum("SPECIAL_ARTILLERY_BARRAGE")]
        ArtilleryBarrage,

        [IniEnum("SPECIAL_CASH_HACK")]
        CashHack,

        [IniEnum("SPECIAL_SPY_SATELLITE")]
        SpySatellite,

        [IniEnum("SPECIAL_SPY_DRONE")]
        SpyDrone,

        [IniEnum("SPECIAL_RADAR_VAN_SCAN")]
        RadarVanScan,

        [IniEnum("SPECIAL_DEFECTOR")]
        Defector,

        [IniEnum("SPECIAL_TERROR_CELL")]
        TerrorCell,

        [IniEnum("SPECIAL_AMBUSH")]
        Ambush,

        [IniEnum("SPECIAL_BLACK_MARKET_NUKE")]
        BlackMarketNuke,

        [IniEnum("SPECIAL_ANTHRAX_BOMB")]
        AnthraxBomb,

        [IniEnum("SPECIAL_MISSILE_DEFENDER_LASER_GUIDED_MISSILES")]
        MissileDefenderLaserGuidedMissiles,

        [IniEnum("SPECIAL_TANKHUNTER_TNT_ATTACK")]
        TankHunterTntAttack,

        [IniEnum("SPECIAL_REMOTE_CHARGES")]
        RemoteCharges,

        [IniEnum("SPECIAL_TIMED_CHARGES")]
        TimedCharges,

        [IniEnum("SPECIAL_HACKER_DISABLE_BUILDING")]
        HackerDisableBuilding,

        [IniEnum("SPECIAL_INFANTRY_CAPTURE_BUILDING")]
        InfantryCaptureBuilding,

        [IniEnum("SPECIAL_BLACKLOTUS_CAPTURE_BUILDING")]
        BlackLotusCaptureBuilding,

        [IniEnum("SPECIAL_BLACKLOTUS_DISABLE_VEHICLE_HACK")]
        BlackLotusDisableVehicleHack,

        [IniEnum("SPECIAL_BLACKLOTUS_STEAL_CASH_HACK")]
        BlackLotusStealCashHack,

        [IniEnum("SPECIAL_CIA_INTELLIGENCE")]
        CiaIntelligence,

        [IniEnum("SPECIAL_REPAIR_VEHICLES")]
        RepairVehicles,

        [IniEnum("SPECIAL_DISGUISE_AS_VEHICLE")]
        DisguiseAsVehicle,

        [IniEnum("SPECIAL_PARTICLE_UPLINK_CANNON")]
        ParticleUplinkCannon,

        [IniEnum("SPECIAL_CASH_BOUNTY")]
        CashBounty,

        [IniEnum("SPECIAL_CHANGE_BATTLE_PLANS")]
        ChangeBattlePlans,

        [IniEnum("SPECIAL_CLEANUP_AREA")]
        CleanupArea,

        [IniEnum("SPECIAL_LAUNCH_BAIKONUR_ROCKET")]
        LaunchBaikonurRocket
    }
}

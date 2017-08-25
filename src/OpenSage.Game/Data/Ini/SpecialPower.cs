using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
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
            { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseAssetReference() },
            { "PublicTimer", (parser, x) => x.PublicTimer = parser.ParseBoolean() },
            { "SharedSyncedTimer", (parser, x) => x.SharedSyncedTimer = parser.ParseBoolean() },
            { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() },
            { "InitiateAtLocationSound", (parser, x) => x.InitiateAtLocationSound = parser.ParseAssetReference() },
            { "ViewObjectDuration", (parser, x) => x.ViewObjectDuration = parser.ParseInteger() },
            { "ViewObjectRange", (parser, x) => x.ViewObjectRange = parser.ParseInteger() },
            { "RadiusCursorRadius", (parser, x) => x.RadiusCursorRadius = parser.ParseInteger() },
            { "ShortcutPower", (parser, x) => x.ShortcutPower = parser.ParseBoolean() },
            { "AcademyClassify", (parser, x) => x.AcademyClassify = parser.ParseEnum<AcademyType>() }
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

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ShortcutPower { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public AcademyType AcademyClassify { get; private set; }
    }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public enum AcademyType
    {
        [IniEnum("ACT_SUPERPOWER")]
        Superpower,

        [IniEnum("ACT_UPGRADE_RADAR")]
        UpgradeRadar
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
        LaunchBaikonurRocket,

        [IniEnum("SPECIAL_CHINA_CARPET_BOMB"), AddedIn(SageGame.CncGeneralsZeroHour)]
        ChinaCarpetBomb,

        [IniEnum("EARLY_SPECIAL_CHINA_CARPET_BOMB"), AddedIn(SageGame.CncGeneralsZeroHour)]
        EarlyChinaCarpetBomb,

        [IniEnum("SPECIAL_LEAFLET_DROP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        LeafletDrop,

        [IniEnum("EARLY_SPECIAL_LEAFLET_DROP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        EarlyLeafletDrop,

        [IniEnum("SPECIAL_SPECTRE_GUNSHIP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SpectreGunship,

        [IniEnum("SPECIAL_FRENZY"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Frenzy,

        [IniEnum("EARLY_SPECIAL_FRENZY"), AddedIn(SageGame.CncGeneralsZeroHour)]
        EarlyFrenzy,

        [IniEnum("SPECIAL_BOOBY_TRAP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        BoobyTrap,

        [IniEnum("SPECIAL_COMMUNICATIONS_DOWNLOAD"), AddedIn(SageGame.CncGeneralsZeroHour)]
        CommunicationsDownload,

        [IniEnum("EARLY_SPECIAL_REPAIR_VEHICLES"), AddedIn(SageGame.CncGeneralsZeroHour)]
        EarlyRepairVehicles,

        [IniEnum("SPECIAL_GPS_SCRAMBLER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        GpsScrambler,

        [IniEnum("SPECIAL_SNEAK_ATTACK"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SneakAttack,

        [IniEnum("SPECIAL_HELIX_NAPALM_BOMB"), AddedIn(SageGame.CncGeneralsZeroHour)]
        HelixNapalmBomb,

        [IniEnum("SPECIAL_BATTLESHIP_BOMBARDMENT"), AddedIn(SageGame.CncGeneralsZeroHour)]
        BattleshipBombardment,

        [IniEnum("SPECIAL_TANK_PARADROP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        TankParadrop,

        [IniEnum("SUPW_SPECIAL_PARTICLE_UPLINK_CANNON"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SuperWeaponParticleUplinkCannon,

        [IniEnum("AIRF_SPECIAL_DAISY_CUTTER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        AirForceDaisyCutter,

        [IniEnum("NUKE_SPECIAL_CLUSTER_MINES"), AddedIn(SageGame.CncGeneralsZeroHour)]
        NukeClusterMines,

        [IniEnum("NUKE_SPECIAL_NEUTRON_MISSILE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        NukeNeutronMissile,

        [IniEnum("AIRF_SPECIAL_A10_THUNDERBOLT_STRIKE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        AirForceA10ThunderboltStrike,

        [IniEnum("AIRF_SPECIAL_SPECTRE_GUNSHIP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        AirForceSpectreGunship,

        [IniEnum("INFA_SPECIAL_PARADROP_AMERICA"), AddedIn(SageGame.CncGeneralsZeroHour)]
        InfantryParadropAmerica,

        [IniEnum("SLTH_SPECIAL_GPS_SCRAMBLER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        StealthGpsScrambler,

        [IniEnum("AIRF_SPECIAL_CARPET_BOMB"), AddedIn(SageGame.CncGeneralsZeroHour)]
        AirForceCarpetBomb,

        [IniEnum("SUPR_SPECIAL_CRUISE_MISSILE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SuperWeaponCruiseMissile,

        [IniEnum("LAZR_SPECIAL_PARTICLE_UPLINK_CANNON"), AddedIn(SageGame.CncGeneralsZeroHour)]
        LaserParticleUplinkCannon,

        [IniEnum("SUPW_SPECIAL_NEUTRON_MISSILE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SuperWeaponNeutronMissile,
    }
}

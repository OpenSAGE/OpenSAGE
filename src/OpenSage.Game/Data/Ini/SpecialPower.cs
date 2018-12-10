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
            { "RadiusCursorRadius", (parser, x) => x.RadiusCursorRadius = parser.ParseFloat() },
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
        public float RadiusCursorRadius { get; private set; }

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

        [IniEnum("SPECIAL_GRAB_PASSENGER"), AddedIn(SageGame.Bfme)]
        SpecialGrabPassenger,

        [IniEnum("SPECIAL_GRAB_CHUNK"), AddedIn(SageGame.Bfme)]
        SpecialGrabChunk,

        [IniEnum("SPECIAL_SPAWN_ORCS"), AddedIn(SageGame.Bfme)]
        SpecialSpawnOrcs,

        [IniEnum("SPECIAL_REPAIR_STRUCTURE"), AddedIn(SageGame.Bfme)]
        SpecialRepairStructure,

        [IniEnum("SPECIAL_SPAWN_OATHBREAKERS"), AddedIn(SageGame.Bfme)]
        SpecialSpawnOathbreakers,

        [IniEnum("SPECIAL_GENERAL_TARGETLESS"), AddedIn(SageGame.Bfme)]
        SpecialGeneralTargetless,

        [IniEnum("SPECIAL_GENERAL_TARGETLESS_TWO"), AddedIn(SageGame.Bfme)]
        SpecialGeneralTargetlessTwo,

        [IniEnum("SPECIAL_BALROG_SCREAM"), AddedIn(SageGame.Bfme)]
        SpecialBalrogScream,

        [IniEnum("SPECIAL_HERO_MODE"), AddedIn(SageGame.Bfme)]
        SpecialHeroMode,

        [IniEnum("SPECIAL_FLAMING_SWORD"), AddedIn(SageGame.Bfme)]
        SpecialFlamingSword,

        [IniEnum("SPECIAL_BALROG_BREATH"), AddedIn(SageGame.Bfme)]
        SpecialBalrogBreath,

        [IniEnum("SPECIAL_KNIFE_ATTACK"), AddedIn(SageGame.Bfme)]
        SpecialKnifeAttack,

        [IniEnum("SPECIAL_ATHELAS"), AddedIn(SageGame.Bfme)]
        SpecialAthelas,

        [IniEnum("SPECIAL_DISGUISE"), AddedIn(SageGame.Bfme)]
        SpecialDisguise,

        [IniEnum("SPECIAL_SMITE_CANCELDISGUISE"), AddedIn(SageGame.Bfme)]
        SpecialSmiteCancelDisguise,

        [IniEnum("SPECIAL_ATTRIBUTEMOD_CANCELDISGUISE"), AddedIn(SageGame.Bfme)]
        SpecialAttributeModCancelDisguise,

        [IniEnum("SPECIAL_FAKE_LEADERSHIP_BUTTON"), AddedIn(SageGame.Bfme)]
        SpecialFakeLeadershipButton,

        [IniEnum("SPECIAL_MTTROLL_BORED"), AddedIn(SageGame.Bfme)]
        SpecialMTTrollBored,

        [IniEnum("SPECIAL_BALROG_WINGS"), AddedIn(SageGame.Bfme)]
        SpecialBalrogWings,

        [IniEnum("SPECIAL_FIRE_WHIP"), AddedIn(SageGame.Bfme)]
        SpecialFireWhip,

        [IniEnum("SPECIAL_WIZARD_BLAST"), AddedIn(SageGame.Bfme)]
        SpecialWizardBlast,

        [IniEnum("SPECIAL_WORD_OF_POWER"), AddedIn(SageGame.Bfme)]
        SpecialWordOfPower,

        [IniEnum("SPECIAL_SHIELD_BUBBLE"), AddedIn(SageGame.Bfme)]
        SpecialShieldBubble,

        [IniEnum("SPECIAL_TELEKENETIC_PUSH"), AddedIn(SageGame.Bfme)]
        SpecialTelekineticPush,

        [IniEnum("SPECIAL_TOGGLE_MOUNTED"), AddedIn(SageGame.Bfme)]
        SpecialToggleMounted,

        [IniEnum("SPECIAL_RAIN_OF_FIRE"), AddedIn(SageGame.Bfme)]
        SpecialRainOfFire,

        [IniEnum("SPECIAL_CHARGE_ATTACK"), AddedIn(SageGame.Bfme)]
        SpecialChargeAttack,

        [IniEnum("SPECIAL_DEFLECT_PROJECTILES"), AddedIn(SageGame.Bfme)]
        SpecialDeflectProjectiles,

        [IniEnum("SPECIAL_SIEGEDEPLOY"), AddedIn(SageGame.Bfme)]
        SpecialSiegedeploy,

        [IniEnum("SPECIAL_STOP"), AddedIn(SageGame.Bfme)]
        SpecialStop,

        [IniEnum("SPECIAL_AT_VISIBLE_OBJECT"), AddedIn(SageGame.Bfme)]
        SpecialAtVisibleObject,

        [IniEnum("SPECIAL_KNIFE_FIGHTER"), AddedIn(SageGame.Bfme)]
        SpecialKnifeFighter,

        [IniEnum("SPECIAL_ARROW_STORM"), AddedIn(SageGame.Bfme)]
        SpecialArrowStorm,

        [IniEnum("SPECIAL_WOUND_ARROW"), AddedIn(SageGame.Bfme)]
        SpecialWoundArrow,

        [IniEnum("SPECIAL_KINGS_FAVOR"), AddedIn(SageGame.Bfme)]
        SpecialKingsFavor,

        [IniEnum("SPECIAL_SWOOP_ATTACK"), AddedIn(SageGame.Bfme)]
        SpecialSwoopAttack,

        [IniEnum("SPECIAL_LEVEL_ATTACK"), AddedIn(SageGame.Bfme)]
        SpecialLevelAttack,

        [IniEnum("SPECIAL_SCREECH"), AddedIn(SageGame.Bfme)]
        SpecialScreech,

        [IniEnum("SPECIAL_LEVEL_POSITION"), AddedIn(SageGame.Bfme)]
        SpecialLevelPosition,

        [IniEnum("SPECIAL_GIVE_UPGRADE"), AddedIn(SageGame.Bfme)]
        SpecialGiveUpgrade,

        [IniEnum("SPECIAL_HARVEST"), AddedIn(SageGame.Bfme)]
        SpecialHarvest,

        [IniEnum("SPECIAL_GIVE_UPGRADE_NEAREST"), AddedIn(SageGame.Bfme)]
        SpecialGiveUpgradeHarvest,

        [IniEnum("SPECIAL_ROUSING_SPEECH"), AddedIn(SageGame.Bfme)]
        SpecialRousingSpeech,

        [IniEnum("SPECIAL_TRIGGER_ATTRIBUTE_MODIFIER"), AddedIn(SageGame.Bfme)]
        SpecialTriggerAttributeModifier,

        [IniEnum("SPECIAL_GLORIOUS_CHARGE"), AddedIn(SageGame.Bfme)]
        SpecialGloriousCharge,

        [IniEnum("SPECIAL_REVEAL_MAP_AREA"), AddedIn(SageGame.Bfme)]
        SpecialRevealMapArea,

        [IniEnum("SPECIAL_PART_THE_HEAVENS"), AddedIn(SageGame.Bfme)]
        SpecialPartTheHeavens,

        [IniEnum("SPECIAL_SPELL_BOOK_BALROG_ALLY"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookBalrogAlly,

        [IniEnum("SPECIAL_SPELL_BOOK_CLOUD_BREAK"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookCloudBreak,

        [IniEnum("SPECIAL_SPELL_BOOK_CALL_THE_HORDE"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookCallTheHord,

        [IniEnum("SPECIAL_SPELL_BOOK_DARKNESS"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookDarkness,

        [IniEnum("SPECIAL_SPELL_BOOK_DEVASTATION"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookDevastation,

        [IniEnum("SPECIAL_SPELL_BOOK_EYE_OF_SAURON"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookEyeOfSauron,

        [IniEnum("SPECIAL_SPELL_BOOK_FREEZING_RAIN"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookFreezingRain,

        [IniEnum("SPECIAL_SPELL_BOOK_FUEL_THE_FIRES"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookFuelTheFires,

        [IniEnum("SPECIAL_SPELL_BOOK_INDUSTRY"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookIndustry,

        [IniEnum("SPECIAL_SPELL_BOOK_PLUS_1_NAZGUL"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookPlus1Nazgul,

        [IniEnum("SPECIAL_SPELL_BOOK_PLUS_100_COMMAND_POINTS"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookPlus100CommandPoints,

        [IniEnum("SPECIAL_SPELL_BOOK_TAINT"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookTaint,

        [IniEnum("SPECIAL_SPELL_BOOK_WAR_CHANT"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookWarChant,

        [IniEnum("SPECIAL_SPELL_BOOK_ANDURIL"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookAnduril,

        [IniEnum("SPECIAL_SPELL_BOOK_GANDALF_THE_WHITE"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookGandalfTheWhite,

        [IniEnum("SPECIAL_SPELL_BOOK_ARMY_OF_THE_DEAD"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookArmyOfTheDead,

        [IniEnum("SPECIAL_SPELL_BOOK_DRAFT"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookDraft,

        [IniEnum("SPECIAL_SPELL_BOOK_EAGLE_ALLIES"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookEagleAllies,

        [IniEnum("SPECIAL_SPELL_BOOK_ELVEN_ALLIES"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookElvenAllies,

        [IniEnum("SPECIAL_SPELL_BOOK_ELVEN_GIFTS"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookElvenGifts,

        [IniEnum("SPECIAL_SPELL_BOOK_ELVEN_WOOD"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookElvenWood,

        [IniEnum("SPECIAL_SPELL_BOOK_ENT_ALLIES"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookEntAllies,

        [IniEnum("SPECIAL_SPELL_BOOK_HEAL"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookHeal,

        [IniEnum("SPECIAL_SPELL_BOOK_ROHAN_ALLIES"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookRohanAllies,

        [IniEnum("SPECIAL_GIMLI_LEAP"), AddedIn(SageGame.Bfme)]
        SpecialGimliLeap,

        [IniEnum("SPECIAL_MAN_THE_WALLS"), AddedIn(SageGame.Bfme)]
        SpecialManTheWalls,

        [IniEnum("SPECIAL_RANGER_AMBUSH"), AddedIn(SageGame.Bfme)]
        SpecialRangerAmbush,

        [IniEnum("SPECIAL_OSGILIATH_VETERANS"), AddedIn(SageGame.Bfme)]
        SpecialOsgiliathVeterans,

        [IniEnum("SPECIAL_PRINCE_OF_DOL_ARMOTH"), AddedIn(SageGame.Bfme)]
        SpecialPrinceOfDolAmroth,

        [IniEnum("SPECIAL_SARUMAN_FIRE_BALL"), AddedIn(SageGame.Bfme)]
        SpecialSarumanFireBall,

        [IniEnum("SPECIAL_SPELL_BOOK_PALANTIR_VISION"), AddedIn(SageGame.Bfme)]
        SpecialSpellBookPalantirVision,

        [IniEnum("SPECIAL_DOMINATE_ENEMY"), AddedIn(SageGame.Bfme)]
        SpecialDominateEnemy,

        [IniEnum("SPECIAL_EAT"), AddedIn(SageGame.Bfme)]
        SpecialEat,
    }
}

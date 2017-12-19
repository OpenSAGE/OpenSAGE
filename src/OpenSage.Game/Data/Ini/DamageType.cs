namespace OpenSage.Data.Ini
{
    public enum DamageType
    {
        [IniEnum("DEFAULT")]
        Default = 0,

        [IniEnum("EXPLOSION")]
        Explosion,

        [IniEnum("CRUSH")]
        Crush,

        [IniEnum("ARMOR_PIERCING")]
        ArmorPiercing,

        [IniEnum("SMALL_ARMS")]
        SmallArms,

        [IniEnum("GATTLING")]
        Gattling, // [sic]

        [IniEnum("RADIATION")]
        Radiation,

        [IniEnum("FLAME")]
        Flame,

        [IniEnum("LASER")]
        Laser,

        [IniEnum("SNIPER")]
        Sniper,

        [IniEnum("POISON")]
        Poison,

        [IniEnum("HEALING")]
        Healing,

        [IniEnum("UNRESISTABLE")]
        Unresistable,

        [IniEnum("WATER")]
        Water,

        [IniEnum("DEPLOY")]
        Deploy,

        [IniEnum("SURRENDER")]
        Surrender,

        [IniEnum("HACK")]
        Hack,

        [IniEnum("KILL_PILOT")]
        KillPilot,

        [IniEnum("PENALTY")]
        Penalty,

        [IniEnum("FALLING")]
        Falling,

        [IniEnum("MELEE")]
        Melee,

        [IniEnum("DISARM")]
        Disarm,

        [IniEnum("HAZARD_CLEANUP")]
        HazardCleanup,

        [IniEnum("INFANTRY_MISSILE")]
        InfantryMissile,

        [IniEnum("AURORA_BOMB")]
        AuroraBomb,

        [IniEnum("LAND_MINE")]
        LandMine,

        [IniEnum("JET_MISSILES")]
        JetMissiles,

        [IniEnum("STEALTHJET_MISSILES")]
        StealthjetMissiles,

        [IniEnum("MOLOTOV_COCKTAIL")]
        MolotovCocktail,

        [IniEnum("COMANCHE_VULCAN")]
        ComancheVulcan,

        [IniEnum("FLESHY_SNIPER")]
        FleshySniper,

        [IniEnum("PARTICLE_BEAM")]
        ParticleBeam,

        [IniEnum("SUBDUAL_MISSILE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SubdualMissile,

        [IniEnum("SUBDUAL_VEHICLE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SubdualVehicle,

        [IniEnum("SUBDUAL_BUILDING"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SubdualBuilding,

        [IniEnum("MICROWAVE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Microwave,

        [IniEnum("STATUS"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Status,

        [IniEnum("KILL_GARRISONED"), AddedIn(SageGame.CncGeneralsZeroHour)]
        KillGarrisoned,

        [IniEnum("SLASH"), AddedIn(SageGame.BattleForMiddleEarth)]
        Slash,

        [IniEnum("PIERCE"), AddedIn(SageGame.BattleForMiddleEarth)]
        Pierce,

        [IniEnum("URUK"), AddedIn(SageGame.BattleForMiddleEarth)]
        Uruk,

        [IniEnum("HERO"), AddedIn(SageGame.BattleForMiddleEarth)]
        Hero,

        [IniEnum("HERO_RANGED"), AddedIn(SageGame.BattleForMiddleEarth)]
        HeroRanged,

        [IniEnum("SIEGE"), AddedIn(SageGame.BattleForMiddleEarth)]
        Siege,

        [IniEnum("SPECIALIST"), AddedIn(SageGame.BattleForMiddleEarth)]
        Specialist,

        [IniEnum("MAGIC"), AddedIn(SageGame.BattleForMiddleEarth)]
        Magic,

        [IniEnum("STRUCTURAL"), AddedIn(SageGame.BattleForMiddleEarth)]
        Structural,

        [IniEnum("CHOP"), AddedIn(SageGame.BattleForMiddleEarth)]
        Chop,

        [IniEnum("FORCE"), AddedIn(SageGame.BattleForMiddleEarth)]
        Force,

        [IniEnum("FLY_INTO"), AddedIn(SageGame.BattleForMiddleEarth)]
        FlyInto,

        [IniEnum("GOOD_ARROW_PIERCE"), AddedIn(SageGame.BattleForMiddleEarth)]
        GoodArrowPierce,

        [IniEnum("EVIL_ARROW_PIERCE"), AddedIn(SageGame.BattleForMiddleEarth)]
        EvilArrowPierce,

        [IniEnum("SWORD_SLASH"), AddedIn(SageGame.BattleForMiddleEarth)]
        SwordSlash,

        [IniEnum("WITCH_KING_MORGUL_BLADE"), AddedIn(SageGame.BattleForMiddleEarth)]
        WitchKingMorgulBlade,

        [IniEnum("REFLECTED"), AddedIn(SageGame.BattleForMiddleEarth)]
        Reflected,

        [IniEnum("BALROG_SWORD"), AddedIn(SageGame.BattleForMiddleEarth)]
        BalrogSword,

        [IniEnum("BALROG_WHIP"), AddedIn(SageGame.BattleForMiddleEarth)]
        BalrogWhip,

        [IniEnum("ELECTRIC"), AddedIn(SageGame.BattleForMiddleEarth)]
        Electric,

        [IniEnum("GIMLI_LEAP"), AddedIn(SageGame.BattleForMiddleEarth)]
        GimliLeap,

        [IniEnum("BIG_ROCK"), AddedIn(SageGame.BattleForMiddleEarth)]
        BigRock,

        [IniEnum("CLUBBING"), AddedIn(SageGame.BattleForMiddleEarth)]
        Clubbing,
    }
}

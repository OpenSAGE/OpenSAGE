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

        [IniEnum("SLASH"), AddedIn(SageGame.Bfme)]
        Slash,

        [IniEnum("PIERCE"), AddedIn(SageGame.Bfme)]
        Pierce,

        [IniEnum("URUK"), AddedIn(SageGame.Bfme)]
        Uruk,

        [IniEnum("HERO"), AddedIn(SageGame.Bfme)]
        Hero,

        [IniEnum("HERO_RANGED"), AddedIn(SageGame.Bfme)]
        HeroRanged,

        [IniEnum("SIEGE"), AddedIn(SageGame.Bfme)]
        Siege,

        [IniEnum("SPECIALIST"), AddedIn(SageGame.Bfme)]
        Specialist,

        [IniEnum("MAGIC"), AddedIn(SageGame.Bfme)]
        Magic,

        [IniEnum("STRUCTURAL"), AddedIn(SageGame.Bfme)]
        Structural,

        [IniEnum("CHOP"), AddedIn(SageGame.Bfme)]
        Chop,

        [IniEnum("FORCE"), AddedIn(SageGame.Bfme)]
        Force,

        [IniEnum("FLY_INTO"), AddedIn(SageGame.Bfme)]
        FlyInto,

        [IniEnum("GOOD_ARROW_PIERCE"), AddedIn(SageGame.Bfme)]
        GoodArrowPierce,

        [IniEnum("EVIL_ARROW_PIERCE"), AddedIn(SageGame.Bfme)]
        EvilArrowPierce,

        [IniEnum("SWORD_SLASH"), AddedIn(SageGame.Bfme)]
        SwordSlash,

        [IniEnum("WITCH_KING_MORGUL_BLADE"), AddedIn(SageGame.Bfme)]
        WitchKingMorgulBlade,

        [IniEnum("REFLECTED"), AddedIn(SageGame.Bfme)]
        Reflected,

        [IniEnum("BALROG_SWORD"), AddedIn(SageGame.Bfme)]
        BalrogSword,

        [IniEnum("BALROG_WHIP"), AddedIn(SageGame.Bfme)]
        BalrogWhip,

        [IniEnum("ELECTRIC"), AddedIn(SageGame.Bfme)]
        Electric,

        [IniEnum("GIMLI_LEAP"), AddedIn(SageGame.Bfme)]
        GimliLeap,

        [IniEnum("BIG_ROCK"), AddedIn(SageGame.Bfme)]
        BigRock,

        [IniEnum("CLUBBING"), AddedIn(SageGame.Bfme)]
        Clubbing,

        [IniEnum("BECOME_UNDEAD"), AddedIn(SageGame.Bfme2)]
        BecomeUndead,

        [IniEnum("CAVALRY_RANGED"), AddedIn(SageGame.Bfme2)]
        CavalryRanged,

        [IniEnum("CAVALRY"), AddedIn(SageGame.Bfme2)]
        Cavalry,

        [IniEnum("LOGICAL_FIRE"), AddedIn(SageGame.Bfme2)]
        LogicalFire,

        [IniEnum("BECOME_UNDEAD_ONCE"), AddedIn(SageGame.Bfme2Rotwk)]
        BecomeUndeadOnce,

        [IniEnum("FROST"), AddedIn(SageGame.Bfme2Rotwk)]
        Frost,
    }
}

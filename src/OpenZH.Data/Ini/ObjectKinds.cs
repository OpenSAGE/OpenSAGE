namespace OpenZH.Data.Ini
{
    public enum ObjectKinds
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("SALVAGER")]
        Salvager,

        [IniEnum("PARACHUTABLE")]
        Parachutable,

        [IniEnum("SELECTABLE")]
        Selectable,

        [IniEnum("PROJECTILE")]
        Projectile,

        [IniEnum("CRATE")]
        Crate,

        [IniEnum("NO_COLLIDE")]
        NoCollide,

        [IniEnum("IMMOBILE")]
        Immobile,

        [IniEnum("CAN_CAST_REFLECTIONS")]
        CanCastReflections,

        [IniEnum("VEHICLE")]
        Vehicle,

        [IniEnum("CLEARED_BY_BUILD")]
        ClearedByBuild,

        [IniEnum("FORCEATTACKABLE")]
        ForceAttackable,

        [IniEnum("CAN_SEE_THROUGH_STRUCTURE")]
        CanSeeThroughStructure,

        [IniEnum("LOW_OVERLAPPABLE")]
        LowOverlappable,

        [IniEnum("INFANTRY")]
        Infantry,

        [IniEnum("CAN_BE_REPULSED")]
        CanBeRepulsed,

        [IniEnum("TRANSPORT")]
        Transport,

        [IniEnum("SMALL_MISSILE")]
        SmallMissile,

        [IniEnum("PRELOAD")]
        Preload,

        [IniEnum("SCORE")]
        Score,

        [IniEnum("CAPTURABLE")]
        Capturable,

        [IniEnum("FS_TECHNOLOGY")]
        FsTechnology,

        [IniEnum("MP_COUNT")]
        MpCount,

        [IniEnum("MP_COUNT_FOR_VICTORY")]
        MpCountForVictory,

        [IniEnum("UNATTACKABLE")]
        Unattackable,

        [IniEnum("ALWAYS_VISIBLE")]
        AlwaysVisible,

        [IniEnum("ALWAYS_SELECTABLE")]
        AlwaysSelectable,

        [IniEnum("ATTACK_NEEDS_LINE_OF_SIGHT")]
        AttackNeedsLineOfSight,

        [IniEnum("HERO")]
        Hero,

        [IniEnum("HULK")]
        Hulk,

        [IniEnum("DONT_AUTO_CRUSH_INFANTRY")]
        DontAutoCrushInfantry,

        [IniEnum("CAN_ATTACK")]
        CanAttack,

        [IniEnum("AIRCRAFT")]
        Aircraft,

        [IniEnum("IGNORED_IN_GUI")]
        IgnoredInGui,

        [IniEnum("CAN_RAPPEL")]
        CanRappel,

        [IniEnum("NO_GARRISON")]
        NoGarrison,

        [IniEnum("IGNORES_SELECT_ALL")]
        IgnoresSelectAll,

        [IniEnum("STEALTH_GARRISON")]
        StealthGarrison,

        [IniEnum("CLICK_THROUGH")]
        ClickThrough,

        [IniEnum("MINE")]
        Mine,

        [IniEnum("HUGE")]
        Huge,

        [IniEnum("WEAPON_SALVAGER")]
        WeaponSalvager,

        [IniEnum("MOB_NEXUS")]
        MobNexus,

        [IniEnum("INSERT")]
        Insert,

        [IniEnum("DRAWABLE_ONLY")]
        DrawableOnly,

        [IniEnum("CLEANUP_HAZARD")]
        CleanupHazard,

        [IniEnum("STICK_TO_TERRAIN_SLOPE")]
        StickToTerrainSlope,
    }
}

﻿namespace OpenSage.Data.Ini
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
        MPCount,

        [IniEnum("MP_COUNT_FOR_VICTORY")]
        MPCountForVictory,

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

        [IniEnum("STRUCTURE")]
        Structure,

        [IniEnum("BRIDGE_TOWER")]
        BridgeTower,

        [IniEnum("NO_HEAL_ICON")]
        NoHealIcon,

        [IniEnum("AIRCRAFT_PATH_AROUND")]
        AircraftPathAround,

        [IniEnum("SUPPLY_SOURCE")]
        SupplySource,

        [IniEnum("SUPPLY_SOURCE_ON_PREVIEW")]
        SupplySourceOnPreview,

        [IniEnum("GARRISONABLE_UNTIL_DESTROYED")]
        GarrisonableUntilDestroyed,

        [IniEnum("IMMUNE_TO_CAPTURE")]
        ImmuneToCapture,

        [IniEnum("TECH_BUILDING")]
        TechBuilding,

        [IniEnum("BRIDGE")]
        Bridge,

        [IniEnum("LANDMARK_BRIDGE")]
        LandmarkBridge,

        [IniEnum("WALK_ON_TOP_OF_WALL")]
        WalkOnTopOfWall,

        [IniEnum("DEFENSIVE_WALL")]
        DefensiveWall,

        [IniEnum("SHOW_PORTRAIT_WHEN_CONTROLLED")]
        ShowPortraitWhenControlled,

        [IniEnum("BOAT")]
        Boat,

        [IniEnum("REBUILD_HOLE")]
        RebuildHole,

        [IniEnum("SCORE_DESTROY")]
        ScoreDestroy,

        [IniEnum("COMMANDCENTER")]
        CommandCenter,

        [IniEnum("FS_FACTORY")]
        FSFactory,

        [IniEnum("AUTO_RALLYPOINT")]
        AutoRallyPoint,

        [IniEnum("SCORE_CREATE")]
        ScoreCreate,

        [IniEnum("FS_POWER")]
        FSPower,

        [IniEnum("POWERED")]
        Powered,

        [IniEnum("DOZER")]
        Dozer,

        [IniEnum("DRONE")]
        Drone,

        [IniEnum("AIRFIELD")]
        Airfield,

        [IniEnum("FS_BASE_DEFENSE")]
        FSBaseDefense,

        [IniEnum("SPAWNS_ARE_THE_WEAPONS")]
        SpawnsAreTheWeapons,

        [IniEnum("CASH_GENERATOR")]
        CashGenerator,

        [IniEnum("CANNOT_BUILD_NEAR_SUPPLIES")]
        CannotBuildNearSupplies,

        [IniEnum("HEAL_PAD")]
        HealPad,

        [IniEnum("REPAIR_PAD")]
        RepairPad,

        [IniEnum("LINEBUILD")]
        LineBuild,

        [IniEnum("BALLISTIC_MISSILE")]
        BallisticMissile,

        [IniEnum("PARACHUTE")]
        Parachute,

        [IniEnum("PRODUCED_AT_HELIPAD")]
        ProducedAtHelipad,

        [IniEnum("HARVESTER")]
        Harvester,

        [IniEnum("INERT")]
        Inert,

        [IniEnum("DISGUISER")]
        Disguiser,

        [IniEnum("HUGE_VEHICLE")]
        HugeVehicle,

        [IniEnum("PORTABLE_STRUCTURE")]
        PortableStructure,

        [IniEnum("SHRUBBERY")]
        Shrubbery,

        [IniEnum("WAVEGUIDE")]
        WaveGuide,

        [IniEnum("REVEAL_TO_ALL")]
        RevealToAll,

        [IniEnum("EMP_HARDENED"), AddedIn(SageGame.CncGeneralsZeroHour)]
        EmpHardened,

        [IniEnum("CANNOT_RETALIATE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        CannotRetaliate,

        [IniEnum("NO_SELECT"), AddedIn(SageGame.CncGeneralsZeroHour)]
        NoSelect,

        [IniEnum("FS_SUPERWEAPON"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSSuperweapon,

        [IniEnum("FS_STRATEGY_CENTER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSStrategyCenter,

        [IniEnum("FS_ADVANCED_TECH"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSAdvancedTech,

        [IniEnum("FS_AIRFIELD"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSAirfield,

        [IniEnum("FS_SUPPLY_CENTER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSSupplyCenter,

        [IniEnum("FS_SUPPLY_DROPZONE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSSupplyDropzone,

        [IniEnum("FS_BARRACKS"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSBarracks,

        [IniEnum("FS_WARFACTORY"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSWarFactory,

        [IniEnum("MONEY_HACKER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        MoneyHacker,

        [IniEnum("CLIFF_JUMPER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        CliffJumper,

        [IniEnum("FS_FAKE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSFake,

        [IniEnum("FS_BLACK_MARKET"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSBlackMarket,

        [IniEnum("DEMOTRAP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        DemoTrap,

        [IniEnum("IGNORE_DOCKING_BONES"), AddedIn(SageGame.CncGeneralsZeroHour)]
        IgnoreDockingBones,

        [IniEnum("ARMOR_SALVAGER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        ArmorSalvager,

        [IniEnum("REVEALS_ENEMY_PATHS"), AddedIn(SageGame.CncGeneralsZeroHour)]
        RevealsEnemyPaths,

        [IniEnum("CONSERVATIVE_BUILDING"), AddedIn(SageGame.CncGeneralsZeroHour)]
        ConservativeBuilding,

        [IniEnum("AIRCRAFT_CARRIER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        AircraftCarrier,

        [IniEnum("FS_INTERNET_CENTER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FSInternetCenter,

        [IniEnum("OPTIMIZED_TREE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        OptimizedTree,

        [IniEnum("PROP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Prop,

        [IniEnum("BLAST_CRATER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        BlastCrater,

        [IniEnum("TECH_BASE_DEFENSE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        TechBaseDefense,

        [IniEnum("BOOBY_TRAP"), AddedIn(SageGame.CncGeneralsZeroHour)]
        BoobyTrap,

        [IniEnum("REJECT_UNMANNED"), AddedIn(SageGame.CncGeneralsZeroHour)]
        RejectUnmanned,

        [IniEnum("TACTICAL_MARKER"), AddedIn(SageGame.Bfme)]
        TacticalMarker,

        [IniEnum("CAVALRY"), AddedIn(SageGame.Bfme)]
        Cavalry,

        [IniEnum("MONSTER"), AddedIn(SageGame.Bfme2)]
        Monster,

        [IniEnum("MACHINE"), AddedIn(SageGame.Bfme2)]
        Machine,

        [IniEnum("HORDE"), AddedIn(SageGame.Bfme2)]
        Horde,

        [IniEnum("IGNORE_FOR_VICTORY"), AddedIn(SageGame.Bfme2)]
        IgnoreForVictory,

        [IniEnum("ECONOMY_STRUCTURE"), AddedIn(SageGame.Bfme2)]
        EconomyStructure,

        [IniEnum("WALL_UPGRADE"), AddedIn(SageGame.Bfme2)]
        WallUpgrade,

        [IniEnum("WALL_HUB"), AddedIn(SageGame.Bfme2)]
        WallHub,

        [IniEnum("WALL_SEGMENT"), AddedIn(SageGame.Bfme2)]
        WallSegment,

        [IniEnum("NOT_AUTOACQUIRABLE"), AddedIn(SageGame.Bfme2)]
        NotAutoAcquirable,

        [IniEnum("BLOCKING_GATE"), AddedIn(SageGame.Bfme)]
        BlockingGate,

        [IniEnum("ENVIRONMENT"), AddedIn(SageGame.Bfme)]
        Environment,

        [IniEnum("CREEP"), AddedIn(SageGame.Bfme)]
        Creep,

        [IniEnum("PATH_THROUGH_EACH_OTHER"), AddedIn(SageGame.Bfme)]
        PathThroughEachOther,

        [IniEnum("THROWN_OBJECT"), AddedIn(SageGame.Bfme)]
        ThrownObject,

        [IniEnum("HOBBIT"), AddedIn(SageGame.Bfme)]
        Hobbit,

        [IniEnum("ROCK_VENDOR"), AddedIn(SageGame.Bfme)]
        RockVendor,

        [IniEnum("WORKING_PASSENGER"), AddedIn(SageGame.Bfme)]
        WorkingPassenger,

        [IniEnum("NO_FREEWILL_ENTER"), AddedIn(SageGame.Bfme)]
        NoFreeWillEnter,

        [IniEnum("ROCK"), AddedIn(SageGame.Bfme)]
        Rock,

        [IniEnum("CLUB"), AddedIn(SageGame.Bfme)]
        Club,

        [IniEnum("CAN_USE_SIEGE_TOWER"), AddedIn(SageGame.Bfme)]
        CanUseSiegeTower,

        [IniEnum("ARCHER"), AddedIn(SageGame.Bfme)]
        Archer,

        [IniEnum("PATH_THROUGH_INFANTRY"), AddedIn(SageGame.Bfme)]
        PathThroughInfantry,

        [IniEnum("NEED_BASE_FOUNDATION"), AddedIn(SageGame.Bfme)]
        NeedBaseFoundation,

        [IniEnum("GARRISON"), AddedIn(SageGame.Bfme)]
        Garrison,

        [IniEnum("CHUNK_VENDOR"), AddedIn(SageGame.Bfme)]
        ChunkVendor,

        [IniEnum("URUK"), AddedIn(SageGame.Bfme)]
        Uruk,

        [IniEnum("ARMY_SUMMARY"), AddedIn(SageGame.Bfme)]
        ArmySummary,

        [IniEnum("CASTLE_KEEP"), AddedIn(SageGame.Bfme)]
        CastleKeep,

        [IniEnum("MOVE_ONLY"), AddedIn(SageGame.Bfme)]
        MoveOnly,

        [IniEnum("HIDE_IF_FOGGED"), AddedIn(SageGame.Bfme)]
        HideIfFogged,

        [IniEnum("BASE_FOUNDATION"), AddedIn(SageGame.Bfme)]
        BaseFoundation,

        [IniEnum("CASTLE_CENTER"), AddedIn(SageGame.Bfme)]
        CastleCenter,

        [IniEnum("BASE_SITE"), AddedIn(SageGame.Bfme)]
        BaseSite,

        [IniEnum("CATAPULT_ROCK"), AddedIn(SageGame.Bfme)]
        CatapultRock,

        [IniEnum("GONDORTOWNSMAN"), AddedIn(SageGame.Bfme)]
        GondorTownsman,

        [IniEnum("GONDORTOWNSMANTORCH"), AddedIn(SageGame.Bfme)]
        GondorTownsmanTorch,

        [IniEnum("GONDORTOWNSWOMAN"), AddedIn(SageGame.Bfme)]
        GondorTownsWoman,

        [IniEnum("GONDORTOWNSPAIR"), AddedIn(SageGame.Bfme)]
        GondorTownsPair,

        [IniEnum("SUPPLY_GATHERING_CENTER"), AddedIn(SageGame.Bfme)]
        SupplyGatheringCenter,

        [IniEnum("FS_CASH_PRODUCER"), AddedIn(SageGame.Bfme)]
        FSCashProducer,

        [IniEnum("INDUSTRY_AFFECTED"), AddedIn(SageGame.Bfme)]
        IndustryAffected,

        [IniEnum("NONOCCLUDING"), AddedIn(SageGame.Bfme)]
        NonOccluding,

        [IniEnum("OPTIMIZED_PROP"), AddedIn(SageGame.Bfme)]
        OptimizedProp,

        [IniEnum("MELEE_HORDE"), AddedIn(SageGame.Bfme)]
        MeleeHorde,

        [IniEnum("ARMY_OF_DEAD"), AddedIn(SageGame.Bfme)]
        ArmyOfDead,

        [IniEnum("INERT_SHROUD_REVEALER"), AddedIn(SageGame.Bfme)]
        InertShroudRevealer,

        [IniEnum("MADE_OF_METAL"), AddedIn(SageGame.Bfme)]
        MadeOfMetal,

        [IniEnum("VITAL_FOR_BASE_SURVIVAL"), AddedIn(SageGame.Bfme)]
        VitalForBaseSurvival,

        [IniEnum("MADE_OF_WOOD"), AddedIn(SageGame.Bfme)]
        MadeOfWood,

        [IniEnum("DO_NOT_CLASSIFY"), AddedIn(SageGame.Bfme)]
        DoNotClassify,

        [IniEnum("FACE_AWAY_FROM_CASTLE_KEEP"), AddedIn(SageGame.Bfme)]
        FaceAwayFromCastleKeep,

        [IniEnum("MADE_OF_DIRT"), AddedIn(SageGame.Bfme)]
        MadeOfDirt,

        [IniEnum("TAINT"), AddedIn(SageGame.Bfme)]
        Taint,

        [IniEnum("MADE_OF_STONE"), AddedIn(SageGame.Bfme)]
        MadeOfStone,

        [IniEnum("BASE_DEFENSE_FOUNDATION"), AddedIn(SageGame.Bfme)]
        BaseDefenseFoundation,

        [IniEnum("BANNER"), AddedIn(SageGame.Bfme)]
        Banner,

        [IniEnum("DEFLECT_BY_SPECIAL_POWER"), AddedIn(SageGame.Bfme)]
        DeflectBySpecialPower,

        [IniEnum("OCL_BIT"), AddedIn(SageGame.Bfme)]
        OCLBit,

        [IniEnum("TAINTEFFECT"), AddedIn(SageGame.Bfme)]
        TaintEffect,

        [IniEnum("GRAB_AND_DROP"), AddedIn(SageGame.Bfme)]
        GrabAndDrop,

        [IniEnum("CAN_ATTACK_WALLS"), AddedIn(SageGame.Bfme)]
        CanAttackWalls,

        [IniEnum("CAN_RIDE_BATTERING_RAM"), AddedIn(SageGame.Bfme)]
        CanRideBatteringRam,

        [IniEnum("SIEGE_TOWER"), AddedIn(SageGame.Bfme)]
        SiegeTower,

        [IniEnum("SIEGE_LADDER"), AddedIn(SageGame.Bfme)]
        SiegeLadder,

        [IniEnum("CAN_RIDE_SIEGE_LADDER"), AddedIn(SageGame.Bfme)]
        CanRideSiegeLadder,

        [IniEnum("DEPLOYED_MINE"), AddedIn(SageGame.Bfme)]
        DeployedMine,

        [IniEnum("MINE_TRIGGER"), AddedIn(SageGame.Bfme)]
        MineTrigger,

        [IniEnum("HAS_HEALTH_BAR"), AddedIn(SageGame.Bfme)]
        HasHealthBar,

        [IniEnum("NOTIFY_OF_PREATTACK"), AddedIn(SageGame.Bfme)]
        NotifyOfPreattack,

        [IniEnum("ORC"), AddedIn(SageGame.Bfme)]
        Orc,

        [IniEnum("PORTER"), AddedIn(SageGame.Bfme)]
        Porter,

        [IniEnum("SCARY"), AddedIn(SageGame.Bfme)]
        Scary,

        [IniEnum("BIG_MONSTER"), AddedIn(SageGame.Bfme)]
        BigMonster,

        [IniEnum("SWARM_DOZER"), AddedIn(SageGame.Bfme)]
        SwarmDozer,

        [IniEnum("MOVE_FOR_NOONE"), AddedIn(SageGame.Bfme)]
        MoveForNoOne,

        [IniEnum("TROLL"), AddedIn(SageGame.Bfme)]
        Troll,

        [IniEnum("TREE"), AddedIn(SageGame.Bfme)]
        Tree,

        [IniEnum("WEBBED"), AddedIn(SageGame.Bfme)]
        Webbed,

        [IniEnum("IGNORE_FOR_EVA_SPEECH_POSITION"), AddedIn(SageGame.Bfme)]
        IgnoreForEvaSpeechPosition,

        [IniEnum("DO_NOT_PICK_ME_WHEN_BUILDING"), AddedIn(SageGame.Bfme)]
        DoNotPickMeWhenBuilding,

        [IniEnum("COMBO_HORDE"), AddedIn(SageGame.Bfme)]
        ComboHorde,

        [IniEnum("SUMMONED"), AddedIn(SageGame.Bfme)]
        Summoned,

        [IniEnum("NO_FORMATION_MOVEMENT"), AddedIn(SageGame.Bfme)]
        NoFormationMovement,

        [IniEnum("ARAGORN"), AddedIn(SageGame.Bfme)]
        Aragorn,

        [IniEnum("GANDALF"), AddedIn(SageGame.Bfme)]
        Gandalf,

        [IniEnum("GIMLI"), AddedIn(SageGame.Bfme)]
        Gimli,

        [IniEnum("BUILD_FOR_FREE"), AddedIn(SageGame.Bfme)]
        BuildForFree,

        [IniEnum("SHRUB"), AddedIn(SageGame.Bfme)]
        Shrub,

        [IniEnum("NO_BASE_CAPTURE"), AddedIn(SageGame.Bfme)]
        NoBaseCapture,

        [IniEnum("CRITTER_EMITTER"), AddedIn(SageGame.Bfme)]
        CritterEmitter,

        [IniEnum("SALT_LICK"), AddedIn(SageGame.Bfme)]
        SaltLick,

        [IniEnum("ALWAYS_SHOW_HOUSE_COLOR"), AddedIn(SageGame.Bfme)]
        AlwaysShowHouseColor,

        [IniEnum("BUFF"), AddedIn(SageGame.Bfme)]
        Buff,

        [IniEnum("SPELL_BOOK"), AddedIn(SageGame.Bfme)]
        SpellBook,

        [IniEnum("NEVER_CULL_FOR_MP"), AddedIn(SageGame.Bfme2)]
        NeverCullForMP,

        [IniEnum("SCALEABLE_WALL"), AddedIn(SageGame.Bfme2)]
        ScaleableWall,

        [IniEnum("DONT_HIDE_IF_FOGGED"), AddedIn(SageGame.Bfme2)]
        DontHideIfFogged,

        [IniEnum("WALL_GATE"), AddedIn(SageGame.Bfme2)]
        WallGate,

        [IniEnum("DOZER_FACTORY"), AddedIn(SageGame.Bfme2)]
        DozerFactory,

        [IniEnum("WB_DISPLAY_SCRIPT_NAME"), AddedIn(SageGame.Bfme2)]
        WbDisplayScriptName,

        [IniEnum("OPTIMIZED_SOUND"), AddedIn(SageGame.Bfme2)]
        OptimizedSound,

        [IniEnum("BATTLE_TOWER"), AddedIn(SageGame.Bfme2)]
        BattleTower,

        [IniEnum("WALL"), AddedIn(SageGame.Bfme2)]
        Wall,

        [IniEnum("LINKED_TO_FLAG"), AddedIn(SageGame.Bfme2)]
        LinkedToFlag,

        [IniEnum("CREATE_A_HERO"), AddedIn(SageGame.Bfme2)]
        CreateAHero,

        [IniEnum("CAN_CLIMB_WALLS"), AddedIn(SageGame.Bfme2)]
        CanClimbWalls,

        [IniEnum("PASS_EXPERIENCE_TO_PRODUCER"), AddedIn(SageGame.Bfme2)]
        PassExperienceToProducer,

        [IniEnum("LARGE_RECTANGLE_PATHFIND"), AddedIn(SageGame.Bfme2)]
        LargeRectanglePathfind,

        [IniEnum("PIKEMAN"), AddedIn(SageGame.Bfme2)]
        Pikeman,

        [IniEnum("PIKE"), AddedIn(SageGame.Bfme2)]
        Pike,

        [IniEnum("LIVING_WORLD_BUILDING_MIRROR"), AddedIn(SageGame.Bfme2)]
        LivingWorldBuildingMirror,

        [IniEnum("CAN_SHOOT_OVER_WALLS"), AddedIn(SageGame.Bfme2)]
        CanShootOverWalls,
    }
}

namespace OpenSage.Logic.Orders
{
    public enum OrderType
    {
        EndGame = 27,

        // Selection
        SetSelection = 1001, // Boolean:True, ObjectId:658 // first parameter is whether to clear existing selection (false in case of shift+click)
        SelectAcrossScreen = 1002, // Boolean:false, ObjectId:671, ... (more ids if more objects are selected) // occurs when selecting a unit and then pressing 'e'
        ClearSelection = 1003,
        Deselect = 1004, // ObjectId: 5 // occurs when shift-clicking a unit that is currently selected

        // Group management
        CreateGroup0 = 1006,
        CreateGroup1 = 1007,
        CreateGroup2 = 1008,
        CreateGroup3 = 1009,
        CreateGroup4 = 1010,
        CreateGroup5 = 1011,
        CreateGroup6 = 1012,
        CreateGroup7 = 1013,
        CreateGroup8 = 1014,
        CreateGroup9 = 1015,
        SelectGroup0 = 1016,
        SelectGroup1 = 1017,
        SelectGroup2 = 1018,
        SelectGroup3 = 1019,
        SelectGroup4 = 1020,
        SelectGroup5 = 1021,
        SelectGroup6 = 1022,
        SelectGroup7 = 1023,
        SelectGroup8 = 1024,
        SelectGroup9 = 1025,

        // the leading integer for these special power arguments seems to correspond to SpecialPowerType
        // burton detonate demo charge      SpecialPower(Integer:25, Integer:256, ObjectId:0)
        // detention center intelligence    SpecialPower(Integer:34, Integer:0, ObjectId:0)
        // strategy center bombardment      SpecialPower(Integer:41, Integer:9216, ObjectId:0)
        // strategy center hold the line    SpecialPower(Integer:41, Integer:17408, ObjectId:0)
        // strategy center search & destroy SpecialPower(Integer:41, Integer:33792, ObjectId:0)
        SpecialPower = 1040,
        // spy satellite             SpecialPowerAtLocation(Integer:15, Position:<304.26398, 409.33313, 10>, ObjectId:0, Integer:544, ObjectId:0)
        // spy drone                 SpecialPowerAtLocation(Integer:16, Position:<825.8123, 606.12665, 10>, ObjectId:0, Integer:928, ObjectId:4) // last object id could be command center source?
        // particle cannon           SpecialPowerAtLocation(Integer:37, Position:<442.46735, 318.8226, 10>, ObjectId:0, Integer:672, ObjectId:0)
        // ambulance cleanup area    SpecialPowerAtLocation(Integer:42, Position:<443.1347, 219.59857, 10>, ObjectId:0, Integer:288, ObjectId:0)
        SpecialPowerAtLocation = 1041,
        // burton remote demo charge SpecialPowerAtObject(Integer:25, ObjectId:2, Integer:771, ObjectId:0) // first object id is target
        // burton timed demo charge  SpecialPowerAtObject(Integer:26, ObjectId:3, Integer:259, ObjectId:0) // first object id is target
        SpecialPowerAtObject = 1042, // Integer:29, ObjectId:199, Integer:323, ObjectId:0
        SetRallyPoint = 1043,
        PurchaseScience = 1044,
        BeginUpgrade = 1045, //encountered while adding landmines to power plant: ObjectId:671,Integer:1604 (mines is Upgrades[13]), also when upgrading usa power plant (ObjectId:673,Integer:1593), (ObjectId:671,Integer:1593), also for flashbangs in the barracks (ObjectId:678,Integer:1594)
        CancelUpgrade = 1046,
        CreateUnit = 1047,
        CancelUnit = 1048,
        BuildObject = 1049,
        CancelBuild = 1051,
        Sell = 1052,
        ExitContainer = 1053, // ObjectId:683 the objectid to remove from the container
        Evacuate = 1054,
        DrawBoxSelection = 1058,
        AttackObject = 1059,
        ForceAttackObject = 1060,
        ForceAttackGround = 1061,
        ResumeBuild = 1065,
        MoveTo = 1068,
        ToggleOvercharge = 1078,
        SetCameraPosition = 1092,
        Checksum = 1095,
        Unknown1097 = 1097,

        //new from Diamond_Extinction_vs_Squaak_Ammo_cc_vs_cu__[GameReplays.org].rep

        Unknown1 = 1,
        Unknown2 = 2,
        Unknown3 = 3,
        Unknown4 = 4,
        Unknown5 = 5,
        Unknown6 = 6,
        Unknown7 = 7,
        Unknown8 = 8,
        Unknown9 = 9,



        Unknown1005 = 1005,
        Unknown1026 = 1026,
        Unknown1027 = 1027,
        Unknown1028 = 1028,
        Unknown1029 = 1029,
        Unknown1030 = 1030,
        Unknown1031 = 1031,
        Unknown1032 = 1032,
        Unknown1033 = 1033,
        Unknown1034 = 1034,
        Unknown1035 = 1035,
        Unknown1036 = 1036,
        Unknown1037 = 1037,
        // dozer clear mines     Integer:0, Position:<1154.5593, 505.26968, 18.75003>, Integer:2147483647, ObjectId:0 // primary weapon is clear mines
        // dragon tank fire wall Integer:1, Position:<530.40765, 607.319, 9.9999695>, Integer:2147483647, ObjectId:0  // secondary weapon is fire wall
        // comanche rocket pods  Integer:2, Position:<373.81445, 256.29944, 10>, Integer:2147483647, ObjectId:0       // tertiary weapon is rocket pods
        UseWeapon = 1038,
        Unknown1039 = 1039,
        Unknown1050 = 1050,

        Unknown1055 = 1055,
        Unknown1056 = 1056,
        CombatDrop = 1057, // ObjectId:2 (target building) // used by USA Chinook

        RepairVehicle = 1062, // ObjectId:3 includes vehicles returning to war factory for repair and helicopters landing and airfields for repair
        Unknown1063 = 1063,
        RepairStructure = 1064, // ObjectId:4 when a dozer is ordered to repair a structure
        Enter = 1066,
        GatherDumpSupplies = 1067, // used for both gathering from a supply source and dumping supplies

        AttackMove = 1069, // Position:<1343.561, 378.53568, 18.75>
        Unknown1070 = 1070,
        AddWaypoint = 1071, // Position:<1147.202, 214.9476, 18.75>
        GuardMode = 1072, // Position:<1256.29822, 505.26968, 18.75>, Integer:0 // integer 0 is guard ground, 2 is guard air
        Unknown1073 = 1073,
        StopMoving = 1074,
        Scatter = 1075, // no arguments
        Unknown1076 = 1076,
        Cheer = 1077, // no arguments

        SelectWeapon = 1079, // Integer:1 // e.g. USA Ranger, 1 for flashbang 0 for machine gun
        Unknown1080 = 1080,
        Unknown1081 = 1081,
        Unknown1082 = 1082,
        Unknown1083 = 1083,
        Unknown1084 = 1084,
        Unknown1085 = 1085,
        DirectParticleCannon = 1086, // Position:<490.00174, 279.01785, 10>, Integer:0, ObjectId:0 // occurs when moving a particle cannon while it is being fired
        Unknown1087 = 1087, //same as 1068
        Unknown1088 = 1088,
        Unknown1089 = 1089, //place beacon? (Position:<1627,444. 386,3608. 20>)
        Unknown1090 = 1090,
        Unknown1091 = 1091,

        Unknown1093 = 1093,
        ToggleFormationMode = 1094, // no arguments

        SelectClearMines = 1096, // no arguments

        Unknown1098 = 1098,
        Unknown1099 = 1099,

        Zero = 0
    }
}

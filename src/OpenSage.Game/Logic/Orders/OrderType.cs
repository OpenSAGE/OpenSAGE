namespace OpenSage.Logic.Orders
{
    public enum OrderType
    {
        EndGame = 27,

        // Selection
        SetSelection = 1001, // Boolean:True, ObjectId:658 // first parameter is whether to clear existing selection (false in case of shift+click)
        SelectAcrossScreen = 1002, // Boolean:false, ObjectId:671, ... (more ids if more objects are selected) // occurs when selecting a unit and then pressing 'e'
        ClearSelection = 1003,

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


        SpecialPower = 1040,
        SpecialPowerAtLocation = 1041,
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



        Unknown1004 = 1004,
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
        UseClearMines = 1038, // Integer:0, Position:<1154.5593, 505.26968, 18.75003>, Integer:2147483647, ObjectId:0
        Unknown1039 = 1039,
        Unknown1040 = 1040,
        Unknown1042 = 1042,
        Unknown1050 = 1050,

        Unknown1053 = 1053,
        Unknown1054 = 1054,
        Unknown1055 = 1055,
        Unknown1056 = 1056,
        Unknown1057 = 1057,

        RepairVehicle = 1062, // ObjectId:3 includes vehicles returning to war factory for repair and helicopters landing and airfields for repair
        Unknown1063 = 1063,
        Unknown1064 = 1064,
        Enter = 1066,
        Unknown1067 = 1067,

        AttackMove = 1069, // Position:<1343.561, 378.53568, 18.75>
        Unknown1070 = 1070,
        AddWaypoint = 1071, // Position:<1147.202, 214.9476, 18.75>
        GuardMode = 1072, // Position:<1256.29822, 505.26968, 18.75>, Integer:0
        Unknown1073 = 1073,
        StopMoving = 1074,
        Scatter = 1075, // no arguments
        Unknown1076 = 1076,
        Cheer = 1077, // no arguments

        Unknown1079 = 1079,
        Unknown1080 = 1080,
        Unknown1081 = 1081,
        Unknown1082 = 1082,
        Unknown1083 = 1083,
        Unknown1084 = 1084,
        Unknown1085 = 1085,
        Unknown1086 = 1086,
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

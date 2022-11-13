namespace OpenSage.Logic.Orders
{
    public enum OrderType
    {
        EndGame = 27,

        // Selection
        SetSelection = 1001,
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
        SpecialPowerAtObject = 1042,
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


        Unknown1002 = 1002,

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
        Unknown1038 = 1038,
        Unknown1039 = 1039,
        Unknown1040 = 1040,
        Unknown1042 = 1042,
        Unknown1050 = 1050,

        Unknown1053 = 1053,
        Unknown1054 = 1054,
        Unknown1055 = 1055,
        Unknown1056 = 1056,
        Unknown1057 = 1057,

        Unknown1062 = 1062,
        Unknown1063 = 1063,
        Unknown1064 = 1064,
        Enter = 1066,
        GatherDumpSupplies = 1067, // used for both gathering from a supply source and dumping supplies

        Unknown1069 = 1069,//AttackMove?
        Unknown1070 = 1070,
        Unknown1071 = 1071,
        Unknown1072 = 1072,
        Unknown1073 = 1073,
        StopMoving = 1074,
        Unknown1075 = 1075,
        Unknown1076 = 1076,
        Unknown1077 = 1077,

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
        Unknown1094 = 1094,

        Unknown1096 = 1096,

        Unknown1098 = 1098,
        Unknown1099 = 1099,

        Zero = 0
    }
}

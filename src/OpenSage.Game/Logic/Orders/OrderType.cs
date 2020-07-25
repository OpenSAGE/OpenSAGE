namespace OpenSage.Logic.Orders
{
    public enum OrderType
    {
        Unknown27 = 27, // Something to do with end of game
        SetSelection = 1001,
        ClearSelection = 1003,
        SpecialPower = 1040,
        SpecialPowerAtLocation = 1041,
        SpecialPowerAtObject = 1042,
        SetRallyPoint = 1043,
        CreateUnit = 1047,
        CancelUnit = 1048,
        BuildObject = 1049,
        Sell = 1052,
        DrawBoxSelection = 1058,
        AttackObject = 1059,
        ForceAttackObject = 1060,
        ForceAttackGround = 1061,
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
        Unknown1006 = 1006,
        Unknown1007 = 1007,
        Unknown1008 = 1008,
        Unknown1009 = 1009,
        Unknown1010 = 1010, //something to do with an object
        Unknown1011 = 1011,
        Unknown1012 = 1012,
        Unknown1013 = 1013,
        Unknown1014 = 1014,
        Unknown1015 = 1015,
        Unknown1016 = 1016,
        Unknown1017 = 1017,
        Unknown1018 = 1018,
        Unknown1019 = 1019,
        Unknown1020 = 1020,
        Unknown1021 = 1021,
        Unknown1022 = 1022,
        Unknown1023 = 1023,
        Unknown1024 = 1024,
        Unknown1025 = 1025,
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
        Unknown1041 = 1041, //encountered when spawning usa spydrone (Integer:16,Position:<1055.208, 466.9498, 18.75>,ObjectId:0,Integer:928,ObjectId:657),
                            //again with  (Integer:16,Position:<1762,799. 3837,869. 100>,ObjectId:0,Integer:928,ObjectId:0)

        Unknown1042 = 1042,

        ChooseGeneralPromotion = 1044, //something with general promotion? gla first promotion -> arg 34, gla second promotion -> arg 35, gla third promotion -> arg 36
        ObjectUprade = 1045, //encountered while adding landmines to power plant: ObjectId:671,Integer:1604 (mines is Upgrades[13]), also when upgrading usa power plant (ObjectId:673,Integer:1593), (ObjectId:671,Integer:1593)
        Unknown1046 = 1046,

        Unknown1050 = 1050,
        Unknown1051 = 1051,

        Unknown1053 = 1053,
        Unknown1054 = 1054,
        Unknown1055 = 1055,
        Unknown1056 = 1056,
        Unknown1057 = 1057,

        Unknown1062 = 1062,
        Unknown1063 = 1063,
        Unknown1064 = 1064,
        Unknown1065 = 1065,
        Unknown1066 = 1066,
        Unknown1067 = 1067,

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

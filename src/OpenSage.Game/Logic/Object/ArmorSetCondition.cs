using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum ArmorSetCondition
    {
        [IniEnum("VETERAN")]
        Veteran,

        [IniEnum("ELITE")]
        Elite,

        [IniEnum("HERO")]
        Hero,

        [IniEnum("PLAYER_UPGRADE")]
        PlayerUpgrade,

        [IniEnum("SECOND_LIFE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SecondLife,

        [IniEnum("CRATE_UPGRADE_ONE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        CrateUpgradeOne,

        [IniEnum("CRATE_UPGRADE_TWO"), AddedIn(SageGame.CncGeneralsZeroHour)]
        CrateUpgradeTwo,

        [IniEnum("ALTERNATE_FORMATION"), AddedIn(SageGame.Bfme)]
        AlternateFormation,

        [IniEnum("AS_TOWER"), AddedIn(SageGame.Bfme)]
        AsTower,

        [IniEnum("MOUNTED"), AddedIn(SageGame.Bfme)]
        Mounted,

        [IniEnum("PLAYER_UPGRADE_2"), AddedIn(SageGame.Bfme)]
        PlayerUpgrade2,

        [IniEnum("CREATE_A_HERO_01"), AddedIn(SageGame.Bfme2)]
        CreateAHero01,

        [IniEnum("CREATE_A_HERO_02"), AddedIn(SageGame.Bfme2)]
        CreateAHero02,

        [IniEnum("CREATE_A_HERO_03"), AddedIn(SageGame.Bfme2)]
        CreateAHero03,

        [IniEnum("CREATE_A_HERO_04"), AddedIn(SageGame.Bfme2)]
        CreateAHero04,

        [IniEnum("CREATE_A_HERO_05"), AddedIn(SageGame.Bfme2)]
        CreateAHero05,

        [IniEnum("CREATE_A_HERO_06"), AddedIn(SageGame.Bfme2)]
        CreateAHero06,

        [IniEnum("CREATE_A_HERO_07"), AddedIn(SageGame.Bfme2Rotwk)]
        CreateAHero07,
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum ArmorSetCondition
    {
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
    }
}

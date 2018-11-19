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
    }
}

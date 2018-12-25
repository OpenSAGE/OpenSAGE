using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum WeaponSetConditions
    {
        [IniEnum("None")]
        None,

        [IniEnum("PLAYER_UPGRADE")]
        PlayerUpgrade,

        [IniEnum("CARBOMB")]
        CarBomb,

        [IniEnum("MINE_CLEARING_DETAIL")]
        MineClearingDetail,

        [IniEnum("CRATEUPGRADE_ONE")]
        CrateUpgradeOne,

        [IniEnum("CRATEUPGRADE_TWO")]
        CrateUpgradeTwo,

        [IniEnum("WEAPON_RIDER1")]
        WeaponRider1,

        [IniEnum("WEAPON_RIDER2")]
        WeaponRider2,

        [IniEnum("WEAPON_RIDER3")]
        WeaponRider3,

        [IniEnum("WEAPON_RIDER4")]
        WeaponRider4,

        [IniEnum("WEAPON_RIDER5")]
        WeaponRider5,

        [IniEnum("WEAPON_RIDER6")]
        WeaponRider6,

        [IniEnum("WEAPON_RIDER7")]
        WeaponRider7,

        [IniEnum("PASSENGER_TYPE_ONE"), AddedIn(SageGame.Bfme)]
        PassengerTypeOne,

        [IniEnum("RAMPAGE"), AddedIn(SageGame.Bfme)]
        Rampage,

        [IniEnum("CLOSE_RANGE"), AddedIn(SageGame.Bfme)]
        CloseRange,

        [IniEnum("CONTESTING_BUILDING"), AddedIn(SageGame.Bfme)]
        ContestingBuilding,

        [IniEnum("SPECIAL_UPGRADE"), AddedIn(SageGame.Bfme)]
        SpecialUpgrade,

        [IniEnum("WEAPONSET_TOGGLE_1"), AddedIn(SageGame.Bfme)]
        WeaponsetToggle1,

        [IniEnum("WEAPONSET_HERO_MODE"), AddedIn(SageGame.Bfme)]
        WeaponsetHeroMode,

        [IniEnum("CONTAINED"), AddedIn(SageGame.Bfme)]
        Contained,

        [IniEnum("MOUNTED"), AddedIn(SageGame.Bfme)]
        Mounted,

        [IniEnum("WEAPONSET_ONE_RING_MODE"), AddedIn(SageGame.Bfme)]
        WeaponsetOneRingMode,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_01"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS01,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_02"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS02,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_03"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS03,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_04"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS04,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_05"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS05,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_06"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS06,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_07"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS07,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_08"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS08,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_09"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS09,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_10"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS10,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_11"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS11,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_12"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS12,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_13"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS13,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_14"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS14,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_15"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS15,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_16"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS16,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_17"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS17,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_18"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS18,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_19"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS19,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_20"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS20,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_21"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS21,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_22"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS22,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_23"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS23,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_24"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS24,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_25"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS25,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_26"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS26,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_27"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS27,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_28"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS28,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_29"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS29,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_30"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS30,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_31"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS31,

        [IniEnum("WEAPONSET_CREATE_A_HERO_WS_32"), AddedIn(SageGame.Bfme2)]
        WeaponsetCreateAHeroWS32,

        [IniEnum("HIDDEN"), AddedIn(SageGame.Bfme2)]
        Hidden,

        [IniEnum("PASSENGER_TYPE_TWO"), AddedIn(SageGame.Bfme2)]
        PassengerTypeTwo,

        [IniEnum("SPECIAL_ONE"), AddedIn(SageGame.Bfme2)]
        SpecialOne,

        [IniEnum("SPECIAL_TWO"), AddedIn(SageGame.Bfme2)]
        SpecialTwo,
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    // we use this in BitArrays everywhere, but there are cases where this is persisted as a uint
    // objects without an explicit ordinal may not be confirmed
    public enum ObjectStatus
    {
        [IniEnum("UNATTACKABLE"), AddedIn(SageGame.Bfme)]
        Unattackable,

        [IniEnum("TOPPLED")]
        Toppled,

        [IniEnum("UNDER_CONSTRUCTION")]
        UnderConstruction = 2, // 4 in sav file

        [IniEnum("UNSELECTABLE")]
        Unselectable = 3, // 8 in sav file

        [IniEnum("DESTROYED")]
        Destroyed = 4, // destroyed civilian building was 16 in sav file

        [IniEnum("DEATH_1"), AddedIn(SageGame.Bfme)]
        Death1,

        [IniEnum("DEATH_2"), AddedIn(SageGame.Bfme)]
        Death2,

        [IniEnum("DEATH_3"), AddedIn(SageGame.Bfme)]
        Death3,

        [IniEnum("DEATH_4"), AddedIn(SageGame.Bfme)]
        Death4,

        [IniEnum("HIJACKED")]
        Hijacked = 9, // 512 in sav file

        [IniEnum("STATUS_RIDER1")]
        StatusRider1,

        [IniEnum("STATUS_RIDER2")]
        StatusRider2,

        [IniEnum("STATUS_RIDER3")]
        StatusRider3,

        [IniEnum("STATUS_RIDER4")]
        StatusRider4,

        [IniEnum("STATUS_RIDER5")]
        StatusRider5,

        [IniEnum("STATUS_RIDER6")]
        StatusRider6,

        [IniEnum("STATUS_RIDER7")]
        StatusRider7,

        [IniEnum("SOLD")]
        Sold = 18, // object being sold was 262152 in sav file - zero-indexed bits 3 and 18

        [IniEnum("INSIDE_GARRISON")]
        InsideGarrison = 21, // ranger in garrison was 2097160 in sav file - zero-indexed bits 3 and 21

        [IniEnum("IS_BRAKING")]
        IsBraking,

        [IniEnum("AIRBORNE_TARGET")]
        AirborneTarget,

        [IniEnum("MASKED")]
        Masked,

        [IniEnum("STATUS_RIDER8")]
        StatusRider8,

        [IniEnum("CAN_ATTACK"), AddedIn(SageGame.Bfme)]
        CanAttack,

        [IniEnum("BLOODTHIRSTY"), AddedIn(SageGame.Bfme)]
        BloodThirsty,

        [IniEnum("ENCLOSED"), AddedIn(SageGame.Bfme)]
        Enclosed,

        [IniEnum("DEPLOYED"), AddedIn(SageGame.Bfme)]
        Deployed,

        [IniEnum("RIDER1"), AddedIn(SageGame.Bfme)]
        Rider1,

        [IniEnum("RIDER2"), AddedIn(SageGame.Bfme)]
        Rider2,

        [IniEnum("RIDERLESS"), AddedIn(SageGame.Bfme)]
        Riderless,

        [IniEnum("HOLDING_THE_RING"), AddedIn(SageGame.Bfme2)]
        HoldingTheRing,

        [IniEnum("IGNORE_AI_COMMAND"), AddedIn(SageGame.Bfme2Rotwk)]
        IgnoreAICommand,

        [IniEnum("SUMMONING_REPLACEMENT"), AddedIn(SageGame.Bfme2Rotwk)]
        SummoningReplacement,

        [IniEnum("USER_DEFINED_1"), AddedIn(SageGame.Bfme2Rotwk)]
        UserDefined1,

        [IniEnum("NO_HERO_PROPERTIES"), AddedIn(SageGame.Bfme2Rotwk)]
        NoHeroProperties,

        [IniEnum("HOLDING_THE_SHARD"), AddedIn(SageGame.Bfme2Rotwk)]
        HoldingTheShard,
    }
}

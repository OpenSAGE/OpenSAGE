using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum ObjectStatus
    {
        [IniEnum("UNDER_CONSTRUCTION")]
        UnderConstruction,

        [IniEnum("HIJACKED")]
        Hijacked,

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

        [IniEnum("TOPPLED")]
        Toppled,

        [IniEnum("INSIDE_GARRISON"), AddedIn(SageGame.Bfme)]
        InsideGarrison,

        [IniEnum("UNSELECTABLE"), AddedIn(SageGame.Bfme)]
        Unselectable,

        [IniEnum("DEATH_1"), AddedIn(SageGame.Bfme)]
        Death1,

        [IniEnum("DEATH_2"), AddedIn(SageGame.Bfme)]
        Death2,

        [IniEnum("DEATH_3"), AddedIn(SageGame.Bfme)]
        Death3,
        
        [IniEnum("DEATH_4"), AddedIn(SageGame.Bfme)]
        Death4,

        [IniEnum("CAN_ATTACK"), AddedIn(SageGame.Bfme)]
        CanAttack,

        [IniEnum("BLOODTHIRSTY"), AddedIn(SageGame.Bfme)]
        BloodThirsty,

        [IniEnum("ENCLOSED"), AddedIn(SageGame.Bfme)]
        Enclosed,

        [IniEnum("UNATTACKABLE"), AddedIn(SageGame.Bfme)]
        Unattackable,

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

        [IniEnum("SOLD"), AddedIn(SageGame.Bfme2)]
        Sold,

        [IniEnum("IGNORE_AI_COMMAND"), AddedIn(SageGame.Bfme2Rotwk)]
        IgnoreAICommand,

        [IniEnum("SUMMONING_REPLACEMENT"), AddedIn(SageGame.Bfme2Rotwk)]
        SummoningReplacement,

        [IniEnum("DESTROYED"), AddedIn(SageGame.Bfme2Rotwk)]
        Destroyed,
    }
}

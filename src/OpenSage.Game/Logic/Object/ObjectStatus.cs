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
        Rider1,

        [IniEnum("STATUS_RIDER2")]
        Rider2,

        [IniEnum("STATUS_RIDER3")]
        Rider3,

        [IniEnum("STATUS_RIDER4")]
        Rider4,

        [IniEnum("STATUS_RIDER5")]
        Rider5,

        [IniEnum("STATUS_RIDER6")]
        Rider6,

        [IniEnum("STATUS_RIDER7")]
        Rider7,

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
    }
}

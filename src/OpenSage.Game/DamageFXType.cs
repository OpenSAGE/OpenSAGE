using OpenSage.Data.Ini;

namespace OpenSage
{
    [AddedIn(SageGame.Bfme)]
    public enum DamageFXType
    {
        [IniEnum("MAGIC")]
        Magic,

        [IniEnum("SWORD_SLASH")]
        SwordSlash,

        [IniEnum("EVIL_ARROW_PIERCE")]
        EvilArrowPierce,

        [IniEnum("CLUBBING")]
        Clubbing,

        [IniEnum("SMALL_ROCK")]
        SmallRock,

        [IniEnum("BIG_ROCK"), AddedIn(SageGame.Bfme2)]
        BigRock,

        [IniEnum("FLAME"), AddedIn(SageGame.Bfme2)]
        Flame,

        [IniEnum("ELECTRIC"), AddedIn(SageGame.Bfme)]
        Electric,

        [IniEnum("BALROG_SWORD"), AddedIn(SageGame.Bfme)]
        BalrogSword,

        [IniEnum("GOOD_ARROW_PIERCE"), AddedIn(SageGame.Bfme)]
        GoodArrowPierce,

        [IniEnum("REFLECTED"), AddedIn(SageGame.Bfme)]
        Reflected,

        [IniEnum("GIMLI_LEAP"), AddedIn(SageGame.Bfme)]
        GimliLeap,

        [IniEnum("WITCH_KING_MORGUL_BLADE"), AddedIn(SageGame.Bfme)]
        WitchKingMorgulBlade,

        [IniEnum("STRUCTURAL"), AddedIn(SageGame.Bfme)]
        Structural,

        [IniEnum("BALROG_WHIP"), AddedIn(SageGame.Bfme)]
        BalrogWhip,

        [IniEnum("POISON"), AddedIn(SageGame.Bfme2)]
        Poison,

        [IniEnum("BOLT"), AddedIn(SageGame.Bfme2)]
        Bolt,

        [IniEnum("TORNADO"), AddedIn(SageGame.Bfme2)]
        Tornado,

        [IniEnum("FIRE1"), AddedIn(SageGame.Bfme2)]
        Fire1,

        [IniEnum("FIRE2"), AddedIn(SageGame.Bfme2)]
        Fire2,

        [IniEnum("FIRE3"), AddedIn(SageGame.Bfme2)]
        Fire3,

        [IniEnum("FLOOD_HORSE"), AddedIn(SageGame.Bfme2)]
        FloodHorse,

        [IniEnum("UNDEFINED"), AddedIn(SageGame.Bfme2)]
        Undefined,

        [IniEnum("NECRO1"), AddedIn(SageGame.Bfme2Rotwk)]
        Necro1,

        [IniEnum("NECRO2"), AddedIn(SageGame.Bfme2Rotwk)]
        Necro2,
    }
}

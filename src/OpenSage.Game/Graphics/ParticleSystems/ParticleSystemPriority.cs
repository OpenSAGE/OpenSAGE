using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems
{
    public enum ParticleSystemPriority
    {
        [IniEnum("NONE"), AddedIn(SageGame.Bfme)]
        None,

        [IniEnum("WEAPON_EXPLOSION", "WEAPON_EXPLSION")]
        WeaponExplosion,

        [IniEnum("SCORCHMARK")]
        ScorchMark,

        [IniEnum("DUST_TRAIL")]
        DustTrail,

        [IniEnum("BUILDUP")]
        Buildup,

        [IniEnum("DEBRIS_TRAIL")]
        DebrisTrail,

        [IniEnum("UNIT_DAMAGE_FX")]
        UnitDamageFX,

        [IniEnum("DEATH_EXPLOSION")]
        DeathExplosion,

        [IniEnum("SEMI_CONSTANT")]
        SemiConstant,

        [IniEnum("CONSTANT")]
        Constant,

        [IniEnum("WEAPON_TRAIL")]
        WeaponTrail,

        [IniEnum("AREA_EFFECT")]
        AreaEffect,

        [IniEnum("CRITICAL")]
        Critical,

        [IniEnum("ALWAYS_RENDER")]
        AlwaysRender,

        [IniEnum("ULTRA_HIGH_ONLY"), AddedIn(SageGame.Bfme2)]
        UltraHighOnly,

        [IniEnum("HIGH_OR_ABOVE"), AddedIn(SageGame.Bfme)]
        HighOrAbove,

        [IniEnum("MEDIUM_OR_ABOVE"), AddedIn(SageGame.Bfme)]
        MediumOrAbove,

        [IniEnum("LOW_OR_ABOVE"), AddedIn(SageGame.Bfme)]
        LowOrAbove,

        [IniEnum("VERY_LOW_OR_ABOVE"), AddedIn(SageGame.Bfme)]
        VeryLowOrAbove,
    }
}

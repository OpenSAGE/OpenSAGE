using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems
{
    public enum ParticleSystemPriority
    {
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

        [IniEnum("HIGH_OR_ABOVE"), AddedIn(SageGame.Bfme)]
        HighOrAbove,

        [IniEnum("MEDIUM_OR_ABOVE"), AddedIn(SageGame.Bfme)]
        MediumOrAbove,

        [IniEnum("VERY_LOW_OR_ABOVE"), AddedIn(SageGame.Bfme)]
        VeryLowOrAbove,

        [IniEnum("NONE"), AddedIn(SageGame.Bfme)]
        None,

        [IniEnum("LOW_OR_ABOVE"), AddedIn(SageGame.Bfme)]
        LowOrAbove,

        [IniEnum("ULTRA_HIGH_ONLY"), AddedIn(SageGame.Bfme2)]
        UltraHighOnly,
    }
}

using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    [Flags]
    public enum AnimationStateTypeFlags
    {
        None = 0,

        [IniEnum("MOVING")]
        Moving = 1 << 0,
        [IniEnum("WANDER")]
        Wander = 1 << 1,
        [IniEnum("BACKING_UP")]
        BackingUp = 1 << 2,
        [IniEnum("FIRING_OR_PREATTACK_A")]
        FiringOrPreattackA = 1 << 3,
        [IniEnum("HARVEST_PREPARATION")]
        HarvestPreparation = 1 << 4,
        [IniEnum("HARVEST_ACTION")]
        HarvestAction = 1 << 5,
        [IniEnum("DYING")]
        Dying = 1 << 6,
        [IniEnum("AFLAME")]
        Aflame = 1 << 7,
        [IniEnum("DEATH_1")]
        Death1 = 1 << 8,
        [IniEnum("DEATH_2")]
        Death2 = 1 << 9,
        [IniEnum("STUNNED_FLAILING")]
        StunnedFlailing = 1 << 10,
        [IniEnum("STUNNED")]
        Stunned = 1 << 11,
        [IniEnum("STUNNED_STANDING_UP")]
        StunnedStandingUp = 1 << 12,
        [IniEnum("EMOTION_AFRAID")]
        EmotionAfraid = 1 << 13,
        [IniEnum("EMOTION_CELEBRATING")]
        EmotionCelebrating = 1 << 14,
        [IniEnum("EMOTION_ALERT")]
        EmotionAlert = 1 << 15,
        [IniEnum("EMOTION_MORALE_HIGH")]
        EmotionMoraleHigh = 1 << 16,
        [IniEnum("HIT_REACTION")]
        HitReaction = 1 << 17,
        [IniEnum("HIT_LEVEL_1")]
        HitLevel1 = 1 << 18,
        [IniEnum("ACTIVELY_CONSTRUCTING")]
        ActivelyConstructing = 1 << 19,
        [IniEnum("SELECTED")]
        Selected = 1 << 20,
        [IniEnum("WEAPONLOCK_PRIMARY")]
        DyingWeaponLockPrimary = 1 << 21,
        [IniEnum("SPLATTED")]
        Splatted = 1 << 22,
        [IniEnum("FIRING_OR_PREATTACK_B")]
        FiringOrPreattackB = 1 << 23,
        [IniEnum("UNPACKING")]
        Unpacking = 1 << 24,
        [IniEnum("PACKING_TYPE_1")]
        PackingType1 = 1 << 25,
        [IniEnum("PREPARING")]
        Preparing = 1 << 26,
        [IniEnum("PACKING")]
        Packing = 1 << 27,
        [IniEnum("PACKING_TYPE_2")]
        PackingType2 = 1 << 28,
        [IniEnum("HIDDEN")]
        Hidden = 1 << 29,
        [IniEnum("USER_1")]
        User1 = 1 << 30,
        [IniEnum("USER_2")]
        User2 = 1 << 31,
    }
}

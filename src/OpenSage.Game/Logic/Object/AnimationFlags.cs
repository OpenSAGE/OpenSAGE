using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum AnimationFlags
    {
        None = 0,

        [IniEnum("RANDOMSTART")]
        RandomStart                         = 1 << 0,

        [IniEnum("START_FRAME_FIRST")]
        StartFrameFirst                     = 1 << 1,

        [IniEnum("START_FRAME_LAST")]
        StartFrameLast                      = 1 << 2,

        [IniEnum("MAINTAIN_FRAME_ACROSS_STATES")]
        MaintainFrameAcrossStates           = 1 << 3,

        [IniEnum("MAINTAIN_FRAME_ACROSS_STATES2")]
        MaintainFrameAcrossStates2          = 1 << 4,

        [IniEnum("MAINTAIN_FRAME_ACROSS_STATES3")]
        MaintainFrameAcrossStates3          = 1 << 5,

        [IniEnum("PRISTINE_BONE_POS_IN_FINAL_FRAME")]
        PristineBonePosInFinalFrame         = 1 << 6,

        [IniEnum("ADJUST_HEIGHT_BY_CONSTRUCTION_PERCENT")]
        AdjustHeightByConstructionPercent   = 1 << 7,

        /// <summary>
        /// This is a hack. <see cref="AnimationMode.Loop"/> can't be used in conjunction with
        /// <see cref="ObjectConditionState.WaitForStateToFinishIfPossible"/>, so this is a
        /// workaround to allow that.
        /// </summary>
        [IniEnum("RESTART_ANIM_WHEN_COMPLETE")]
        RestartAnimWhenComplete             = 1 << 8,

        [IniEnum("MAINTAIN_FRAME_ACROSS_STATES4"), AddedIn(SageGame.Bfme)]
        MaintainFrameAcrossStates4          = 1 << 9,
    }
}

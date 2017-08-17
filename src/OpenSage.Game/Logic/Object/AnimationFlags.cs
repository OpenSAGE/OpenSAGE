using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum AnimationFlags
    {
        None = 0,

        [IniEnum("RANDOMSTART")]
        RandomStart = 1 << 0,

        [IniEnum("START_FRAME_FIRST")]
        StartFrameFirst = 1 << 1,

        [IniEnum("START_FRAME_LAST")]
        StartFrameLast = 1 << 2,

        [IniEnum("MAINTAIN_FRAME_ACROSS_STATES")]
        MaintainFrameAcrossStates = 1 << 3,

        [IniEnum("ADJUST_HEIGHT_BY_CONSTRUCTION_PERCENT")]
        AdjustHeightByConstructionPercent = 1 << 4
    }
}

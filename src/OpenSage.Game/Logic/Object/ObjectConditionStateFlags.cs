using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum ObjectConditionStateFlags
    {
        None = 0,

        [IniEnum("RANDOMSTART")]
        RandomStart = 1 << 0,

        [IniEnum("START_FRAME_FIRST")]
        StartFrameFirst = 1 << 1,

        [IniEnum("MAINTAIN_FRAME_ACROSS_STATES")]
        MaintainFrameAcrossStates = 1 << 2
    }
}

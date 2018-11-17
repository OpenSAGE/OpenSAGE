using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum AnimationStateTypeFlags
    {
        None = 0,

        [IniEnum("MOVING")]
        Moving = 1 << 0,
    }
}

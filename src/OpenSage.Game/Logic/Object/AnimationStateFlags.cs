using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    [Flags]
    public enum AnimationStateFlags
    {
        None = 0,

        [IniEnum("RANDOMSTART")]
        Randomstart = 1 << 0,
    }
}

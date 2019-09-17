using System;
using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    [Flags]
    public enum AudioControlFlags
    {
        [IniEnum("none")]
        None = 0,

        [IniEnum("loop")]
        Loop = 1 << 0,

        [IniEnum("all")]
        All = 1 << 1,

        [IniEnum("interrupt")]
        Interrupt = 1 << 2,

        [IniEnum("random")]
        Random = 1 << 3,

        [IniEnum("randomstart")]
        RandomStart = 1 << 4,

        [IniEnum("fade_on_kill")]
        FadeOnKill = 1 << 5,

        [IniEnum("fade_on_start")]
        FadeOnStart = 1 << 6,

        [IniEnum("sequential"), AddedIn(SageGame.Bfme2)]
        Sequential = 1 << 7,
    }
}

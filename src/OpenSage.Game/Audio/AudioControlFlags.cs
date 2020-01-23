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

        [IniEnum("sequential"), AddedIn(SageGame.Bfme2)]
        Sequential = 1 << 1,

        [IniEnum("randomstart")]
        RandomStart = 1 << 2,

        [IniEnum("interrupt")]
        Interrupt = 1 << 3,

        [IniEnum("fade_on_kill")]
        FadeOnKill = 1 << 4,

        [IniEnum("fade_on_start")]
        FadeOnStart = 1 << 5,

        AllowKillMidFile = 1 << 6,

        [IniEnum("all")]
        All = 1 << 7,

        [IniEnum("random")]
        Random = 1 << 8,
    }
}

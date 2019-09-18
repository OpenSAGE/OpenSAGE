using System;
using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    [Flags]
    public enum AudioTypeFlags
    {
        None = 0,

        [IniEnum("ui")]
        UI = 1,

        [IniEnum("world")]
        World = 2,

        [IniEnum("shrouded")]
        Shrouded = 4,

        [IniEnum("voice")]
        Voice = 8,

        [IniEnum("player")]
        Player = 16,

        [IniEnum("allies")]
        Allies = 32,

        [IniEnum("enemies")]
        Enemies = 64,

        [IniEnum("everyone")]
        Everyone = 128,

        [IniEnum("default")]
        Default = 256,

        [IniEnum("global")]
        Global = 512,

        [IniEnum("FAKE"), AddedIn(SageGame.Bfme)]
        Fake = 1024,
    }
}

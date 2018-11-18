using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum ObjectBrowserFlags
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("CINEMATICS")]
        Cinematics          = 1 << 0,

        [IniEnum("UNIT")]
        Unit                = 1 << 1,

        [IniEnum("MISC_NATURAL"), AddedIn(SageGame.Bfme)]
        MiscNatural         = 1 << 2,

        [IniEnum("SYSTEM"), AddedIn(SageGame.Bfme)]
        System              = 1 << 3,

        [IniEnum("MISC_MAN_MADE"), AddedIn(SageGame.Bfme)]
        MiscManMade         = 1 << 4,

        [IniEnum("STRUCTURE"), AddedIn(SageGame.Bfme)]
        Structure           = 1 << 5,

        [IniEnum("ATMOSPHERIC_EFFECTS"), AddedIn(SageGame.Bfme)]
        AtmosphericEffects  = 1 << 6,

        [IniEnum("SKYBOXES"), AddedIn(SageGame.Bfme)]
        Skyboxes            = 1 << 7,

        [IniEnum("SHRUBBERY"), AddedIn(SageGame.Bfme)]
        Shrubbery            = 1 << 8,
    }
}

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
        Cinematics              = 1 << 0,

        [IniEnum("UNIT")]
        Unit                    = 1 << 1,

        [IniEnum("MISC_NATURAL"), AddedIn(SageGame.Bfme)]
        MiscNatural             = 1 << 2,

        [IniEnum("SYSTEM"), AddedIn(SageGame.Bfme)]
        System                  = 1 << 3,

        [IniEnum("MISC_MAN_MADE"), AddedIn(SageGame.Bfme)]
        MiscManMade             = 1 << 4,

        [IniEnum("STRUCTURE"), AddedIn(SageGame.Bfme)]
        Structure               = 1 << 5,

        [IniEnum("ATMOSPHERIC_EFFECTS"), AddedIn(SageGame.Bfme)]
        AtmosphericEffects      = 1 << 6,

        [IniEnum("SKYBOXES"), AddedIn(SageGame.Bfme)]
        Skyboxes                = 1 << 7,

        [IniEnum("SHRUBBERY"), AddedIn(SageGame.Bfme)]
        Shrubbery               = 1 << 8,

        [IniEnum("REGION"), AddedIn(SageGame.Bfme)]
        Region                  = 1 << 9,

        [IniEnum("CIRITH_UNGOL"), AddedIn(SageGame.Bfme)]
        CirithUngol             = 1 << 10,

        [IniEnum("LOTHLORIEN"), AddedIn(SageGame.Bfme)]
        Lothlorien              = 1 << 11,

        [IniEnum("HELMS_DEEP"), AddedIn(SageGame.Bfme)]
        HelmsDeep               = 1 << 12,

        [IniEnum("PROPS"), AddedIn(SageGame.Bfme)]
        Props                   = 1 << 13,

        [IniEnum("DEAD_MARSHES"), AddedIn(SageGame.Bfme)]
        DeadMarshes             = 1 << 14,

        [IniEnum("NEXT"), AddedIn(SageGame.Bfme)]
        Next                    = 1 << 15,

        [IniEnum("PATHS_OF_DEAD"), AddedIn(SageGame.Bfme)]
        PathsOfDead             = 1 << 16,

        [IniEnum("FANGORN_FOREST"), AddedIn(SageGame.Bfme)]
        FangornForest           = 1 << 17,

        [IniEnum("RIVENDELL"), AddedIn(SageGame.Bfme)]
        Rivendell               = 1 << 18,

        [IniEnum("MINAS_TIRITH"), AddedIn(SageGame.Bfme)]
        MinasTirith             = 1 << 19,

        [IniEnum("OSGILIATH"), AddedIn(SageGame.Bfme)]
        Osgiliath               = 1 << 20,

        [IniEnum("ROHAN"), AddedIn(SageGame.Bfme)]
        Rohan                   = 1 << 21,

        [IniEnum("AI"), AddedIn(SageGame.Bfme)]
        Ai                      = 1 << 22,

        [IniEnum("TACTICAL_MARKERS"), AddedIn(SageGame.Bfme)]
        TacticalMarkers         = 1 << 23,

        [IniEnum("OBSOLETE"), AddedIn(SageGame.Bfme2)]
        Obsolete                = 1 << 24,

        [IniEnum("MINAS_MORGUL"), AddedIn(SageGame.Bfme2)]
        Minas_Morgul            = 1 << 25,

        [IniEnum("SHIRE"), AddedIn(SageGame.Bfme2)]
        Shire                   = 1 << 26,
    }
}

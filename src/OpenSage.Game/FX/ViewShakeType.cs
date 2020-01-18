using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public enum ViewShakeType
    {
        [IniEnum("SUBTLE")]
        Subtle,

        [IniEnum("NORMAL")]
        Normal,

        [IniEnum("STRONG")]
        Strong,

        [IniEnum("SEVERE")]
        Severe,

        [IniEnum("CINE_EXTREME"), AddedIn(SageGame.Bfme)]
        CineExtreme
    }
}

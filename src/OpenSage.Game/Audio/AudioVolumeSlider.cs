using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    [AddedIn(SageGame.Bfme)]
    public enum AudioVolumeSlider
    {
        [IniEnum("SOUNDFX")]
        SoundFX,

        [IniEnum("VOICE")]
        Voice,

        [IniEnum("MUSIC")]
        Music,

        [IniEnum("AMBIENT")]
        Ambient,

        [IniEnum("MOVIE")]
        Movie,

        [IniEnum("None"), AddedIn(SageGame.Bfme)]
        None,
    }
}

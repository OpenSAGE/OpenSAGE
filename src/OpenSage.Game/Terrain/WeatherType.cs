using OpenSage.Data.Ini;

namespace OpenSage.Terrain
{
    [AddedIn(SageGame.Bfme)]
    public enum WeatherType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("RAINY")]
        Rainy,

        [IniEnum("CLOUDYRAINY")]
        CloudyRainy,

        [IniEnum("SUNNY")]
        Sunny,

        [IniEnum("CLOUDY")]
        Cloudy,

        [IniEnum("NORMAL"), AddedIn(SageGame.Bfme2)]
        Normal,

        [IniEnum("SNOWY"), AddedIn(SageGame.Bfme2)]
        Snowy,
    }
}

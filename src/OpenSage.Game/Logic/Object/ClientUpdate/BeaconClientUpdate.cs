using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to produce the BeaconSmokeFFFFFF particle system definition by default but will 
    /// call the BeaconSmoke###### particle system definition relative to the player's color.
    /// </summary>
    public sealed class BeaconClientUpdateModuleData : ClientUpdateModuleData
    {
        internal static BeaconClientUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<BeaconClientUpdateModuleData> FieldParseTable = new IniParseTable<BeaconClientUpdateModuleData>
        {
            { "RadarPulseFrequency", (parser, x) => x.RadarPulseFrequency = parser.ParseInteger() },
            { "RadarPulseDuration", (parser, x) => x.RadarPulseDuration = parser.ParseInteger() }
        };

        public int RadarPulseFrequency { get; private set; }
        public int RadarPulseDuration { get; private set; }
    }
}

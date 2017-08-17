using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of the RADAR_EXTENDING and RADAR_UPGRADED model condition states and enables 
    /// the Radar in the command bar.
    /// </summary>
    public sealed class RadarUpdate : ObjectBehavior
    {
        internal static RadarUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RadarUpdate> FieldParseTable = new IniParseTable<RadarUpdate>
        {
            { "RadarExtendTime", (parser, x) => x.RadarExtendTime = parser.ParseInteger() }
        };
        
        public int RadarExtendTime { get; private set; }
    }
}

using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of the RADAR_EXTENDING and RADAR_UPGRADED model condition states and enables 
    /// the Radar in the command bar.
    /// </summary>
    public sealed class RadarUpdateBehavior : ObjectBehavior
    {
        internal static RadarUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RadarUpdateBehavior> FieldParseTable = new IniParseTable<RadarUpdateBehavior>
        {
            { "RadarExtendTime", (parser, x) => x.RadarExtendTime = parser.ParseInteger() }
        };
        
        public int RadarExtendTime { get; private set; }
    }
}

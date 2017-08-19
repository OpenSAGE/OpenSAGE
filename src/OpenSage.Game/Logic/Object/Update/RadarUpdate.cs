using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
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

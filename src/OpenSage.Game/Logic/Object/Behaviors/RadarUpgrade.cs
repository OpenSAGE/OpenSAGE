using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of RadarUpdate module on this object if present and enables the Radar in the 
    /// command bar.
    /// </summary>
    public sealed class RadarUpgrade : ObjectBehavior
    {
        internal static RadarUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RadarUpgrade> FieldParseTable = new IniParseTable<RadarUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
    }
}

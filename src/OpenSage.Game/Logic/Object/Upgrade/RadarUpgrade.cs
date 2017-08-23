using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of <see cref="RadarUpdate"/> module on this object if present and enables the 
    /// Radar in the command bar.
    /// </summary>
    public sealed class RadarUpgrade : ObjectBehavior
    {
        internal static RadarUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RadarUpgrade> FieldParseTable = new IniParseTable<RadarUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "DisableProof", (parser, x) => x.DisableProof = parser.ParseBoolean() }
        };

        public string TriggeredBy { get; private set; }
        public bool DisableProof { get; private set; }
    }
}

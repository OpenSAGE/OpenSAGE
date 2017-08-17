using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of RadarUpdate module on this object if present and enables the Radar in the 
    /// command bar.
    /// </summary>
    public sealed class RadarUpgradeBehavior : ObjectBehavior
    {
        internal static RadarUpgradeBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RadarUpgradeBehavior> FieldParseTable = new IniParseTable<RadarUpgradeBehavior>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
    }
}

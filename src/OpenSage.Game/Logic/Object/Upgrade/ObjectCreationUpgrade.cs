using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows an object to create/spawn a new object via upgrades.
    /// </summary>
    public sealed class ObjectCreationUpgrade : ObjectBehavior
    {
        internal static ObjectCreationUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ObjectCreationUpgrade> FieldParseTable = new IniParseTable<ObjectCreationUpgrade>
        {
            { "UpgradeObject", (parser, x) => x.UpgradeObject = parser.ParseAssetReference() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReference() }
        };

        public string UpgradeObject { get; private set; }
        public string TriggeredBy { get; private set; }
        public string ConflictsWith { get; private set; }
    }
}

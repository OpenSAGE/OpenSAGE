using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of PLAYER_UPGRADE ArmorSet on this object.
    /// </summary>
    public sealed class ArmorUpgrade : ObjectBehavior
    {
        internal static ArmorUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ArmorUpgrade> FieldParseTable = new IniParseTable<ArmorUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
        };

        public string TriggeredBy { get; private set; }
        public string[] ConflictsWith { get; private set; }
    }
}

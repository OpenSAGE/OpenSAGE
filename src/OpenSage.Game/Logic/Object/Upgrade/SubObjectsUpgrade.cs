using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Shows/hides sub-objects on this object's model via upgrading.
    /// </summary>
    public sealed class SubObjectsUpgrade : ObjectBehavior
    {
        internal static SubObjectsUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SubObjectsUpgrade> FieldParseTable = new IniParseTable<SubObjectsUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "ShowSubObjects", (parser, x) => x.ShowSubObjects = parser.ParseAssetReferenceArray() },
            { "HideSubObjects", (parser, x) => x.HideSubObjects = parser.ParseAssetReferenceArray() },
            { "RequiresAllTriggers", (parser, x) => x.RequiresAllTriggers = parser.ParseBoolean() }
        };

        public string[] TriggeredBy { get; private set; }
        public string[] ConflictsWith { get; private set; }
        public string[] ShowSubObjects { get; private set; }
        public string[] HideSubObjects { get; private set; }
        public bool RequiresAllTriggers { get; private set; }
    }
}

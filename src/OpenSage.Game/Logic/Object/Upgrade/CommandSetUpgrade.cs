using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Gives the object the ability to change commandsets back and forth when this module two times.
    /// </summary>
    public sealed class CommandSetUpgrade : ObjectBehavior
    {
        internal static CommandSetUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CommandSetUpgrade> FieldParseTable = new IniParseTable<CommandSetUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "CommandSet", (parser, x) => x.CommandSet = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
        public string[] ConflictsWith { get; private set; }
        public string CommandSet { get; private set; }
    }
}

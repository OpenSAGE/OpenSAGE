using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// The contained module will be replaced by any module of the same exact class.
    /// </summary>
    public sealed class OverrideableByLikeKind
    {
        internal static OverrideableByLikeKind Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<OverrideableByLikeKind> FieldParseTable = new IniParseTable<OverrideableByLikeKind>
        {
            { "Behavior", (parser, x) => x.Module = ObjectBehavior.ParseBehavior(parser) },
        };

        public ObjectModule Module { get; private set; }
    }
}

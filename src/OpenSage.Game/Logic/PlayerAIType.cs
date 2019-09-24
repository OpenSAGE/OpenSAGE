using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme)]
    public sealed class PlayerAIType : BaseAsset
    {
        internal static PlayerAIType Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("PlayerAIType", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<PlayerAIType> FieldParseTable = new IniParseTable<PlayerAIType>
        {
            { "LibraryMap", (parser, x) => x.LibraryMap = parser.ParseQuotedString() },
        };

        public string LibraryMap { get; private set; }
    }
}

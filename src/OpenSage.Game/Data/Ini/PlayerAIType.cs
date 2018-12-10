using System.Numerics;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class PlayerAIType
    {
        internal static PlayerAIType Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<PlayerAIType> FieldParseTable = new IniParseTable<PlayerAIType>
        {
            { "LibraryMap", (parser, x) => x.LibraryMap = parser.ParseQuotedString() },
        };

        public string Name { get; private set; }

        public string LibraryMap { get; private set; }
    }
}

using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class Pathfinder
    {
        internal static Pathfinder Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<Pathfinder> FieldParseTable = new IniParseTable<Pathfinder>
        {
            { "SlopeLimits", (parser, x) => x.SlopeLimits = parser.ParseFloatArray() },
        };

        public float[] SlopeLimits { get; private set; }
    }
}

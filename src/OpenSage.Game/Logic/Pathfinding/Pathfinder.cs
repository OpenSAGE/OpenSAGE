using OpenSage.Data.Ini;

namespace OpenSage.Logic.Pathfinding
{
    [AddedIn(SageGame.Bfme)]
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

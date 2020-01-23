using OpenSage.Data.Ini;

namespace OpenSage.Logic.Pathfinding
{
    [AddedIn(SageGame.Bfme)]
    public sealed class Pathfinder : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, Pathfinder value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<Pathfinder> FieldParseTable = new IniParseTable<Pathfinder>
        {
            { "SlopeLimits", (parser, x) => x.SlopeLimits = parser.ParseFloatArray() },
        };

        public float[] SlopeLimits { get; private set; }
    }
}

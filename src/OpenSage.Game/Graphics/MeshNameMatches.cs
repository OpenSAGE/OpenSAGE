using OpenSage.Data.Ini;

namespace OpenSage.Graphics
{
    //Setup file for the instancing manager.
    [AddedIn(SageGame.Bfme2)]
    public sealed class MeshNameMatches
    {
        internal static MeshNameMatches Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MeshNameMatches> FieldParseTable = new IniParseTable<MeshNameMatches>
        {
            { "Instances", (parser, x) => x.Instances = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public int Instances { get; private set; }
    }
}

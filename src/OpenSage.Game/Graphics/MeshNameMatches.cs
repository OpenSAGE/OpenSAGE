using OpenSage.Data.Ini;

namespace OpenSage.Graphics
{
    //Setup file for the instancing manager.
    [AddedIn(SageGame.Bfme2)]
    public sealed class MeshNameMatches : BaseAsset
    {
        internal static MeshNameMatches Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("MeshNameMatches", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<MeshNameMatches> FieldParseTable = new IniParseTable<MeshNameMatches>
        {
            { "Instances", (parser, x) => x.Instances = parser.ParseInteger() },
        };

        public int Instances { get; private set; }
    }
}

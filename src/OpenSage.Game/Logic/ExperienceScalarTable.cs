using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ExperienceScalarTable : BaseAsset
    {
        internal static ExperienceScalarTable Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("ExperienceScalarTable", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<ExperienceScalarTable> FieldParseTable = new IniParseTable<ExperienceScalarTable>
        {
            { "Scalars", (parser, x) => x.Scalars = parser.ParseFloatArray() }
        };

        public float[] Scalars { get; private set; }
    }
}

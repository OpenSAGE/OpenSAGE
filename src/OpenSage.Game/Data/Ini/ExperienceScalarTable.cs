using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ExperienceScalarTable
    {
        internal static ExperienceScalarTable Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<ExperienceScalarTable> FieldParseTable = new IniParseTable<ExperienceScalarTable>
        {
            { "Scalars", (parser, x) => x.Scalars = parser.ParseFloatArray() }
        };

        public string Name { get; private set; }

        public float[] Scalars { get; private set; }
    }
}

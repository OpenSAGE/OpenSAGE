using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ExperienceScalarUpgrade : ObjectBehavior
    {
        internal static ExperienceScalarUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ExperienceScalarUpgrade> FieldParseTable = new IniParseTable<ExperienceScalarUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "AddXPScalar", (parser, x) => x.AddXPScalar = parser.ParseFloat() }
        };

        public string TriggeredBy { get; private set; }
        public float AddXPScalar { get; private set; }
    }
}

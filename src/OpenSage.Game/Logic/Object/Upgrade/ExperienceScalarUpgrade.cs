using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ExperienceScalarUpgradeModuleData : UpgradeModuleData
    {
        internal static ExperienceScalarUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ExperienceScalarUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ExperienceScalarUpgradeModuleData>
            {
                { "AddXPScalar", (parser, x) => x.AddXPScalar = parser.ParseFloat() }
            });

        public float AddXPScalar { get; private set; }
    }
}

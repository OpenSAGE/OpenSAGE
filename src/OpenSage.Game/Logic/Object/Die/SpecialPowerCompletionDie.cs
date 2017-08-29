using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpecialPowerCompletionDieModuleData : DieModuleData
    {
        internal static SpecialPowerCompletionDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpecialPowerCompletionDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<SpecialPowerCompletionDieModuleData>
            {
                { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() }
            });

        public string SpecialPowerTemplate { get; private set; }
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HeroDieModuleData : DieModuleData
    {
        internal static  HeroDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable< HeroDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable< HeroDieModuleData>
            {
                { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() }
            });

        public string SpecialPowerTemplate { get; private set; }
    }
}

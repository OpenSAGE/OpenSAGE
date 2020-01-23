using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CombineHordeSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new CombineHordeSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CombineHordeSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CombineHordeSpecialPowerModuleData>
            {
                { "ScanRange", (parser, x) => x.ScanRange = parser.ParseFloat() }
            });

        public float ScanRange { get; private set; }
    }
}

using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class UntamedAllegianceSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new UntamedAllegianceSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UntamedAllegianceSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<UntamedAllegianceSpecialPowerModuleData>
            {
            });
    }
}

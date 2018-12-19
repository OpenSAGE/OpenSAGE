using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class HordeDispatchSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new HordeDispatchSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HordeDispatchSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<HordeDispatchSpecialPowerModuleData>());
    }
}

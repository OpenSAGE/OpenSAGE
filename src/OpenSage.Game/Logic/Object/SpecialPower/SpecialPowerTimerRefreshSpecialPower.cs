using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class SpecialPowerTimerRefreshSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new SpecialPowerTimerRefreshSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpecialPowerTimerRefreshSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<SpecialPowerTimerRefreshSpecialPowerModuleData>());
    }
}

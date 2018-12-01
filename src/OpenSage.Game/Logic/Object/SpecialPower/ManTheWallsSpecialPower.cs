using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ManTheWallsSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new ManTheWallsSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable< ManTheWallsSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable< ManTheWallsSpecialPowerModuleData>());
    }
}

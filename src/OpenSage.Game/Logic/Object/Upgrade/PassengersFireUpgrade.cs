using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Contain modules should have the "PassengersAllowedToFire" parameter set to "No" in order 
    /// for this module to work.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class PassengersFireUpgradeModuleData : UpgradeModuleData
    {
        internal static PassengersFireUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<PassengersFireUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<PassengersFireUpgradeModuleData>());
    }
}

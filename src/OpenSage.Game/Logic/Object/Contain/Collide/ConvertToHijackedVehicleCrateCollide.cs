using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to play the HijackDriver sound definition when triggered and converts the unit to 
    /// your side.
    /// </summary>
    public sealed class ConvertToHijackedVehicleCrateCollideModuleData : CrateCollideModuleData
    {
        internal static ConvertToHijackedVehicleCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ConvertToHijackedVehicleCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<ConvertToHijackedVehicleCrateCollideModuleData>());
    }
}

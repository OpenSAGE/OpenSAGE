using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of CARBOMB WeaponSet Condition of the hijacked object and turns it to a 
    /// suicide unit unless given with a different weapon.
    /// </summary>
    public sealed class ConvertToCarBombCrateCollideModuleData : CrateCollideModuleData
    {
        internal static ConvertToCarBombCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ConvertToCarBombCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<ConvertToCarBombCrateCollideModuleData>());
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of the <see cref="ObjectDefinition.EnergyBonus"/> setting on this object to 
    /// provide extra power to the faction.
    /// </summary>
    public sealed class PowerPlantUpgradeModuleData : UpgradeModuleData
    {
        internal static PowerPlantUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<PowerPlantUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<PowerPlantUpgradeModuleData>());
    }
}

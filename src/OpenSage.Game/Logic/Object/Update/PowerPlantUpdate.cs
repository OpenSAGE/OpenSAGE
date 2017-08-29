using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows this object to act as an (upgradeable) power supply, and allows this object to use 
    /// the <see cref="ModelConditionFlag.PowerPlantUpgrading"/> and 
    /// <see cref="ModelConditionFlag.PowerPlantUpgraded"/> model condition states.
    /// </summary>
    public sealed class PowerPlantUpdateModuleData : UpdateModuleData
    {
        internal static PowerPlantUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PowerPlantUpdateModuleData> FieldParseTable = new IniParseTable<PowerPlantUpdateModuleData>
        {
            { "RodsExtendTime", (parser, x) => x.RodsExtendTime = parser.ParseInteger() }
        };

        public int RodsExtendTime { get; private set; }
    }
}

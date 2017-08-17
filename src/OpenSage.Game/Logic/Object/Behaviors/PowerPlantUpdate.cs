using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows this object to act as an (upgradeable) power supply, and allows this object to use 
    /// the <see cref="ModelConditionFlag.PowerPlantUpgrading"/> and 
    /// <see cref="ModelConditionFlag.PowerPlantUpgraded"/> model condition states.
    /// </summary>
    public sealed class PowerPlantUpdate : ObjectBehavior
    {
        internal static PowerPlantUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PowerPlantUpdate> FieldParseTable = new IniParseTable<PowerPlantUpdate>
        {
            { "RodsExtendTime", (parser, x) => x.RodsExtendTime = parser.ParseInteger() }
        };

        public int RodsExtendTime { get; private set; }
    }
}

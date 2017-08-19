using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Displays GUI:OverchargeExhausted when object's health is below specified percentage.
    /// Allows use of the <see cref="ObjectDefinition.EnergyBonus"/> parameter.
    /// Allows this object to turn on or off the <see cref="ModelConditionFlag.PowerPlantUpgrading"/>
    /// and <see cref="ModelConditionFlag.PowerPlantUpgraded"/> condition states.
    /// </summary>
    public sealed class OverchargeBehavior : ObjectBehavior
    {
        internal static OverchargeBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<OverchargeBehavior> FieldParseTable = new IniParseTable<OverchargeBehavior>
        {
            { "HealthPercentToDrainPerSecond", (parser, x) => x.HealthPercentToDrainPerSecond = parser.ParsePercentage() },
            { "NotAllowedWhenHealthBelowPercent", (parser, x) => x.NotAllowedWhenHealthBelowPercent = parser.ParsePercentage() }
        };

        /// <summary>
        /// Percentage of max health to drain per second when in overcharge mode.
        /// </summary>
        public float HealthPercentToDrainPerSecond { get; private set; }

        /// <summary>
        /// Turn off overcharge bonus when object's current health is below this value.
        /// </summary>
        public float NotAllowedWhenHealthBelowPercent { get; private set; }
    }
}

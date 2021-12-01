using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class OverchargeBehavior : UpdateModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknown = reader.ReadBoolean();
            if (unknown)
            {
                throw new InvalidStateException();
            }
        }
    }

    /// <summary>
    /// Displays GUI:OverchargeExhausted when object's health is below specified percentage.
    /// Allows use of the <see cref="ObjectDefinition.EnergyBonus"/> parameter.
    /// Allows this object to turn on or off the <see cref="ModelConditionFlag.PowerPlantUpgrading"/>
    /// and <see cref="ModelConditionFlag.PowerPlantUpgraded"/> condition states.
    /// </summary>
    public sealed class OverchargeBehaviorModuleData : BehaviorModuleData
    {
        internal static OverchargeBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<OverchargeBehaviorModuleData> FieldParseTable = new IniParseTable<OverchargeBehaviorModuleData>
        {
            { "HealthPercentToDrainPerSecond", (parser, x) => x.HealthPercentToDrainPerSecond = parser.ParsePercentage() },
            { "NotAllowedWhenHealthBelowPercent", (parser, x) => x.NotAllowedWhenHealthBelowPercent = parser.ParsePercentage() }
        };

        /// <summary>
        /// Percentage of max health to drain per second when in overcharge mode.
        /// </summary>
        public Percentage HealthPercentToDrainPerSecond { get; private set; }

        /// <summary>
        /// Turn off overcharge bonus when object's current health is below this value.
        /// </summary>
        public Percentage NotAllowedWhenHealthBelowPercent { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new OverchargeBehavior();
        }
    }
}

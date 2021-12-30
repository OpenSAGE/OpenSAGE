using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class PowerPlantUpgrade : UpgradeModule
    {
        internal PowerPlantUpgrade(GameObject gameObject, PowerPlantUpgradeModuleData moduleData) : base(gameObject, moduleData)
        {
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                _gameObject.EnergyProduction += _gameObject.Definition.EnergyBonus;
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Triggers use of the <see cref="ObjectDefinition.EnergyBonus"/> setting on this object to 
    /// provide extra power to the faction.
    /// </summary>
    public sealed class PowerPlantUpgradeModuleData : UpgradeModuleData
    {
        internal static PowerPlantUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<PowerPlantUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<PowerPlantUpgradeModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new PowerPlantUpgrade(gameObject, this);
        }
    }
}

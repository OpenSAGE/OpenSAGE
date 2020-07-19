using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{

    public sealed class PowerPlantUpgrade : UpgradeModule
    {
        private readonly GameObject _gameObject;

        internal PowerPlantUpgrade(GameObject gameObject, PowerPlantUpgradeModuleData moduleData) : base(moduleData)
        {
            _gameObject = gameObject;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (triggered)
            {
                _gameObject.EnergyProduction += _gameObject.Definition.EnergyBonus;
            }
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

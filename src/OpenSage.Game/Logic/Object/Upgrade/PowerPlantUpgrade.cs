using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class PowerPlantUpgrade : UpgradeModule
    {
        internal PowerPlantUpgrade(GameObject gameObject, PowerPlantUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        protected override void OnUpgrade()
        {
            _gameObject.EnergyProduction += _gameObject.Definition.EnergyBonus;

            foreach (var powerPlantUpdate in _gameObject.FindBehaviors<PowerPlantUpdate>())
            {
                powerPlantUpdate.ExtendRods();
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
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

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SupplyWarehouseCreate : CreateModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    /// <summary>
    /// Ensures the object acts as a source for supply collection.
    /// </summary>
    public sealed class SupplyWarehouseCreateModuleData : CreateModuleData
    {
        internal static SupplyWarehouseCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyWarehouseCreateModuleData> FieldParseTable = new IniParseTable<SupplyWarehouseCreateModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SupplyWarehouseCreate();
        }
    }
}

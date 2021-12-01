using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SupplyCenterCreate : CreateModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Requires the <see cref="ObjectKinds.SupplySource"/> KindOf defined in order to work properly.
    /// Ensures the object acts as a destination for collection of supplies.
    /// </summary>
    public sealed class SupplyCenterCreateModuleData : CreateModuleData
    {
        internal static SupplyCenterCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyCenterCreateModuleData> FieldParseTable = new IniParseTable<SupplyCenterCreateModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SupplyCenterCreate();
        }
    }
}

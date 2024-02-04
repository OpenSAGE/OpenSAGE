using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ConvertToHijackedVehicleCrateCollide : CrateCollide
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
    /// Hardcoded to play the HijackDriver sound definition when triggered and converts the unit to
    /// your side.
    /// </summary>
    public sealed class ConvertToHijackedVehicleCrateCollideModuleData : CrateCollideModuleData
    {
        internal static ConvertToHijackedVehicleCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ConvertToHijackedVehicleCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<ConvertToHijackedVehicleCrateCollideModuleData>());

        internal override ConvertToHijackedVehicleCrateCollide CreateModule(GameObject gameObject, GameContext context)
        {
            return new ConvertToHijackedVehicleCrateCollide();
        }
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ConvertToCarBombCrateCollide : CrateCollide
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Triggers use of CARBOMB WeaponSet Condition of the hijacked object and turns it to a 
    /// suicide unit unless given with a different weapon.
    /// </summary>
    public sealed class ConvertToCarBombCrateCollideModuleData : CrateCollideModuleData
    {
        internal static ConvertToCarBombCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ConvertToCarBombCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<ConvertToCarBombCrateCollideModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ConvertToCarBombCrateCollide();
        }
    }
}

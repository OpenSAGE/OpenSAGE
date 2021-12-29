using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class DamDie : DieModule
    {
        // TODO

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Allows object to continue to exist as an obstacle but allowing water terrain to move 
    /// through. The module must be applied after any other death modules.
    /// </summary>
    public sealed class DamDieModuleData : DieModuleData
    {
        internal static DamDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DamDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<DamDieModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DamDie();
        }
    }
}

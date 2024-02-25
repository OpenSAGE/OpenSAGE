using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class DamDie : DieModule
    {
        // TODO
        public DamDie(DamDieModuleData moduleData) : base(moduleData)
        {
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
    /// Allows object to continue to exist as an obstacle but allowing water terrain to move
    /// through. The module must be applied after any other death modules.
    /// </summary>
    public sealed class DamDieModuleData : DieModuleData
    {
        internal static DamDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DamDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<DamDieModuleData>());
    }
}

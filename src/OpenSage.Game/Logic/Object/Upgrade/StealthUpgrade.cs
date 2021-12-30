using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class StealthUpgrade : UpgradeModule
    {
        public StealthUpgrade(GameObject gameObject, StealthUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Eenables use of <see cref="StealthUpdateModuleData"/> module on this object. Requires 
    /// <see cref="StealthUpdateModuleData.InnateStealth"/> = No defined in the <see cref="StealthUpdateModuleData"/> 
    /// module.
    /// </summary>
    public sealed class StealthUpgradeModuleData : UpgradeModuleData
    {
        internal static StealthUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StealthUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<StealthUpgradeModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new StealthUpgrade(gameObject, this);
        }
    }
}

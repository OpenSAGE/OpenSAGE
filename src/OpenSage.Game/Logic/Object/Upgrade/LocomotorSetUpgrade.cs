using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class LocomotorSetUpgrade : UpgradeModule
    {
        public LocomotorSetUpgrade(GameObject gameObject, LocomotorSetUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
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
    /// Triggers use of SET_NORMAL_UPGRADED locomotor on this object and allows the use of 
    /// VoiceMoveUpgrade within the UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class LocomotorSetUpgradeModuleData : UpgradeModuleData
    {
        internal static LocomotorSetUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<LocomotorSetUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<LocomotorSetUpgradeModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new LocomotorSetUpgrade(gameObject, this);
        }
    }
}

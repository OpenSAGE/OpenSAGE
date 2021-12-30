namespace OpenSage.Logic.Object
{
    public abstract class DamageModule : BehaviorModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);
        }
    }

    public abstract class DamageModuleData : ContainModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Damage;
    }
}

namespace OpenSage.Logic.Object
{
    public abstract class DamageModule : BehaviorModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    public abstract class DamageModuleData : ContainModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Damage;
    }
}

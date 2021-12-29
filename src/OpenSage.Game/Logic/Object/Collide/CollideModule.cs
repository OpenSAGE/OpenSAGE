namespace OpenSage.Logic.Object
{
    public abstract class CollideModule : BehaviorModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    public abstract class CollideModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Collide;
    }
}

namespace OpenSage.Logic.Object
{
    public abstract class CollideModule : BehaviorModule
    {
        internal override void Load(SaveFileReader reader)
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

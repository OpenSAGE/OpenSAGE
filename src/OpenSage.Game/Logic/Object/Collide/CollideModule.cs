using System.Numerics;

namespace OpenSage.Logic.Object
{
    public abstract class CollideModule : BehaviorModule, ICollideModule
    {
        // TODO: Make this abstract.
        public virtual void OnCollide(GameObject collidingObject) { }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public abstract class CollideModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Collide;
    }

    public interface ICollideModule
    {
        void OnCollide(GameObject collidingObject);
    }
}

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

            reader.PersistBase(base.Load);
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

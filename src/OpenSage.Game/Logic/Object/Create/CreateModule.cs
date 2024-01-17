namespace OpenSage.Logic.Object
{
    public abstract class CreateModule : BehaviorModule, ICreateModule
    {
        private bool _shouldCallOnBuildComplete = true;

        public virtual void OnCreate() { }

        public void OnBuildComplete()
        {
            if (!_shouldCallOnBuildComplete)
            {
                return;
            }

            _shouldCallOnBuildComplete = false;

            OnBuildCompleteImpl();
        }

        protected virtual void OnBuildCompleteImpl() { }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistBoolean(ref _shouldCallOnBuildComplete);
        }
    }

    public abstract class CreateModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Create;
    }

    public interface ICreateModule
    {
        void OnCreate();
        void OnBuildComplete();
    }
}

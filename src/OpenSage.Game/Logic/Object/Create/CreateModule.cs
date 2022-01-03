namespace OpenSage.Logic.Object
{
    public abstract class CreateModule : BehaviorModule
    {
        private bool _unknown;

        internal virtual void Execute(BehaviorUpdateContext context) { }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistBoolean("Unknown", ref _unknown);
        }
    }

    public abstract class CreateModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Create;
    }
}

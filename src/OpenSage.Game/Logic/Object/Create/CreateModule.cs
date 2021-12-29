namespace OpenSage.Logic.Object
{
    public abstract class CreateModule : BehaviorModule
    {
        private bool _unknown;

        internal virtual void Execute(BehaviorUpdateContext context) { }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadBoolean(ref _unknown);
        }
    }

    public abstract class CreateModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Create;
    }
}

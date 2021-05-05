namespace OpenSage.Logic.Object
{
    public abstract class CreateModule : BehaviorModule
    {
        internal virtual void Execute(BehaviorUpdateContext context) { }
    }

    public abstract class CreateModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Create;
    }
}

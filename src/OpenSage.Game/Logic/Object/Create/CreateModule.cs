namespace OpenSage.Logic.Object
{
    public abstract class CreateModule : BehaviorModule
    {
        internal virtual void Execute(BehaviorUpdateContext context) { }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownBool = reader.ReadBoolean();
        }
    }

    public abstract class CreateModuleData : BehaviorModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Create;
    }
}

namespace OpenSage.Logic.Object
{
    public abstract class UpdateModule : BehaviorModule
    {
        private uint _frameSomething;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            // Maybe some kind of frame timer? But sometimes it's -2.
            reader.PersistFrame("FrameSomething", ref _frameSomething);
        }
    }

    public abstract class UpdateModuleData : ContainModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Update;
    }
}

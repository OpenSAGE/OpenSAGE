namespace OpenSage.Logic.Object
{
    public abstract class UpdateModule : BehaviorModule
    {
        private int _frameSomething;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            // Maybe some kind of frame timer? But sometimes it's -2.
            _frameSomething = reader.ReadInt32();
        }
    }

    public abstract class UpdateModuleData : ContainModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Update;
    }
}

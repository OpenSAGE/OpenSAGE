namespace OpenSage.Logic.Object
{
    public abstract class UpdateModule : BehaviorModule
    {
        private UpdateFrame _updateFrame;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistFrame(ref _updateFrame.RawValue, "UpdateFrame");
        }

        private struct UpdateFrame
        {
            public uint RawValue;

            public uint Frame
            {
                get => RawValue >> 2;
                set => RawValue = (value << 2) | (RawValue & 0x3);
            }

            public byte Something
            {
                get => (byte)(RawValue & 0x3);
                set => RawValue = (RawValue & 0xFFFFFFFC) | (value);
            }
        }
    }

    public abstract class UpdateModuleData : ContainModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Update;
    }
}

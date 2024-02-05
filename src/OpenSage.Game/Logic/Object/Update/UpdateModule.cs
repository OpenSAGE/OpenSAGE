using ImGuiNET;

namespace OpenSage.Logic.Object
{
    public abstract class UpdateModule : BehaviorModule
    {
        // it's also possible this is _last_ update, not next update
        protected UpdateFrame NextUpdateFrame;

        protected virtual uint FramesBetweenUpdates => 1;

        private protected virtual void RunUpdate(BehaviorUpdateContext context) { }

        // todo: seal this method?
        internal override void Update(BehaviorUpdateContext context)
        {
            if (context.LogicFrame.Value < NextUpdateFrame.Frame)
            {
                return;
            }

            NextUpdateFrame.Frame = context.LogicFrame.Value + FramesBetweenUpdates;
            RunUpdate(context);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistFrame(ref NextUpdateFrame.RawValue, "UpdateFrame");
        }

        protected struct UpdateFrame
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

        internal override void DrawInspector()
        {
            ImGui.LabelText("Next update frame", NextUpdateFrame.Frame.ToString());
        }
    }

    public abstract class UpdateModuleData : ContainModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Update;
    }
}

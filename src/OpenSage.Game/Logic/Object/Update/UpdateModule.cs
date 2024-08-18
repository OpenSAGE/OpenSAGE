using ImGuiNET;

namespace OpenSage.Logic.Object
{
    public abstract class UpdateModule : BehaviorModule
    {
        // it's also possible this is _last_ update, not next update
        protected UpdateFrame NextUpdateFrame;

        protected virtual LogicFrameSpan FramesBetweenUpdates { get; } = new(1);

        private protected virtual void RunUpdate(BehaviorUpdateContext context) { }

        // todo: seal this method?
        internal override void Update(BehaviorUpdateContext context)
        {
            if (context.LogicFrame.Value < NextUpdateFrame.Frame)
            {
                return;
            }

            NextUpdateFrame = new UpdateFrame(context.LogicFrame + FramesBetweenUpdates);
            RunUpdate(context);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistUpdateFrame(ref NextUpdateFrame);
        }

        internal override void DrawInspector()
        {
            ImGui.LabelText("Next update frame", NextUpdateFrame.Frame.ToString());
        }
    }

    public struct UpdateFrame
    {
        public uint RawValue;

        public UpdateFrame(LogicFrame frame)
        {
            Frame = frame.Value;
        }

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

    public abstract class UpdateModuleData : ContainModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Update;
    }
}

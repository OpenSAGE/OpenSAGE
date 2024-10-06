using ImGuiNET;

namespace OpenSage.Logic.Object
{
    public abstract class UpdateModule : BehaviorModule, IUpdateModule
    {
        private UpdateFrame _nextUpdateFrame;

        protected UpdateFrame NextUpdateFrame => _nextUpdateFrame;

        protected virtual LogicFrameSpan FramesBetweenUpdates { get; } = new(1);

        protected virtual UpdateOrder UpdateOrder => UpdateOrder.Order2;

        protected UpdateModule()
        {
            _nextUpdateFrame.UpdateOrder = UpdateOrder;
        }

        private protected virtual void RunUpdate(BehaviorUpdateContext context) { }

        void IUpdateModule.Update(BehaviorUpdateContext context)
        {
            Update(context);
        }

        // todo: seal this method?
        internal virtual void Update(BehaviorUpdateContext context)
        {
            if (context.LogicFrame.Value < _nextUpdateFrame.Frame)
            {
                return;
            }

            SetNextUpdateFrame(context.LogicFrame + FramesBetweenUpdates);
            RunUpdate(context);
        }

        protected void SetNextUpdateFrame(LogicFrame frame)
        {
            _nextUpdateFrame = new UpdateFrame(frame, UpdateOrder);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            var currentUpdateOrder = _nextUpdateFrame.UpdateOrder;
            reader.PersistUpdateFrame(ref _nextUpdateFrame);
            if (reader.Mode == StatePersistMode.Read && _nextUpdateFrame.UpdateOrder != currentUpdateOrder)
            {
                throw new InvalidStateException();
            }
        }

        internal override void DrawInspector()
        {
            ImGui.LabelText("Next update frame", _nextUpdateFrame.Frame.ToString());
        }
    }

    public struct UpdateFrame
    {
        public uint RawValue;

        public UpdateFrame(LogicFrame frame, UpdateOrder updateOrder)
        {
            Frame = frame.Value;
            UpdateOrder = updateOrder;
        }

        public uint Frame
        {
            get => RawValue >> 2;
            set => RawValue = (value << 2) | (RawValue & 0x3);
        }

        public UpdateOrder UpdateOrder
        {
            get => (UpdateOrder)(RawValue & 0x3);
            set => RawValue = (RawValue & 0xFFFFFFFC) | (byte)(value);
        }
    }

    public enum UpdateOrder : byte
    {
        Order0 = 0,
        Order1 = 1,
        Order2 = 2,
        Order3 = 3,
    }

    internal interface IUpdateModule
    {
        void Update(BehaviorUpdateContext context);
    }

    public abstract class UpdateModuleData : ContainModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Update;
    }
}

using ImGuiNET;

namespace OpenSage.Logic.Object
{
    public abstract class UpdateModule : BehaviorModule
    {
        // it's also possible this is _last_ update, not next update
        protected UpdateFrame NextUpdateFrame;

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

using System;
using OpenSage.Data.Apt.FrameItems;

namespace OpenSage.Tools.AptEditor.Apt.Editor.FrameItems
{
    class LogicalClipEvent
    {
        private readonly Action<IEditAction> _edit;

        public ClipEvent Linked { get; }
        public ClipEventFlags Flags => Linked.Flags;
        public byte KeyCode => Linked.KeyCode;
        public LogicalInstructions Instructions { get; private set; }

        public LogicalClipEvent(Action<IEditAction> edit, ClipEvent c)
        {
            _edit = edit;
            Linked = c;
            Instructions = new LogicalInstructions(c.Instructions);
        }

        public void SetFlags(ClipEventFlags flags)
        {
            MakeEdit("Set clip event flags", f =>
            {
                var previous = Flags;
                Linked.Flags = f;
                return previous;
            }, flags);
        }

        public void SetKeyCode(byte keyCode)
        {
            MakeEdit("Set clip event keycode", k =>
            {
                var previous = KeyCode;
                Linked.KeyCode = k;
                return previous;
            }, keyCode);
        }

        public void SetInstructions(LogicalInstructions instructions)
        {
            MakeEdit("Set clip event instructions", i =>
            {
                var previous = Instructions;
                Linked.Instructions = i.ConvertToRealInstructions();
                Instructions = i;
                return previous;
            }, instructions);
        }

        private void MakeEdit<T>(string description, Func<T, T> edit, T state) => _edit(new EditAction<T>(edit, state, description));
    }
}

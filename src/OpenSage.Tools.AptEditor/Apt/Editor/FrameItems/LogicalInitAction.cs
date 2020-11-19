using OpenSage.Data.Apt.FrameItems;

namespace OpenSage.Tools.AptEditor.Apt.Editor.FrameItems
{
    internal class LogicalInitAction
    {
        public uint Sprite { get; private set; }
        public LogicalInstructions Instructions { get; private set; }
        public LogicalInitAction(InitAction initAction)
        {
            Sprite = initAction.Sprite;
            Instructions = new LogicalInstructions(initAction.Instructions);
        }
    }
}

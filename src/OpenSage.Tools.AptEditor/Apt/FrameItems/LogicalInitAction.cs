using OpenSage.FileFormats.Apt.FrameItems;
using OpenSage.Tools.AptEditor.ActionScript;

namespace OpenSage.Tools.AptEditor.Apt.Editor.FrameItems
{
    internal class LogicalInitAction
    {
        public uint Sprite { get; private set; }
        public InstructionGraph Instructions { get; private set; }
        public LogicalInitAction(InitAction initAction)
        {
            Sprite = initAction.Sprite;
            Instructions = new InstructionGraph(initAction.Instructions);
        }
    }
}

using Action = OpenSage.FileFormats.Apt.FrameItems.Action;
using OpenSage.Tools.AptEditor.ActionScript;

namespace OpenSage.Tools.AptEditor.Apt.Editor.FrameItems
{
    internal class LogicalAction
    {
        public InstructionGraph Instructions { get; set; }
        public LogicalAction(Action action)
        {
            Instructions = new InstructionGraph(action.Instructions);
        }
    }
}

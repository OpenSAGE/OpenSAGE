using Action = OpenSage.Data.Apt.FrameItems.Action;

namespace OpenSage.Tools.AptEditor.Apt.Editor.FrameItems
{
    internal class LogicalAction
    {
        public LogicalInstructions Instructions { get; set; }
        public LogicalAction(Action action)
        {
            Instructions = new LogicalInstructions(action.Instructions);
        }
    }
}

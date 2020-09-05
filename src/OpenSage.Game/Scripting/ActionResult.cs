namespace OpenSage.Scripting
{
    // type ActionResult = ActionFinished | ActionContinuation
    public class ActionResult
    {
        public static readonly ActionResult Finished = new ActionFinished();

        // This is a zero-sized type which is used to signal that action execution has finished.
        public sealed class ActionFinished : ActionResult { }

        // This is effectively a stackless coroutine - update runs for one cycle, and then yields control back to the scripting system.
        public abstract class ActionContinuation : ActionResult
        {
            public abstract ActionResult Execute(ScriptExecutionContext context);
        }
    }
}

using OpenSage.Data.Map;

namespace OpenSage.Scripting.Actions
{
    public static class ScriptActions
    {
        public static ActionResult EnableScript(ScriptAction action, ScriptExecutionContext context)
        {
            var scriptName = action.Arguments[0].StringValue;
            context.Scripting.EnableScript(scriptName);
            return ActionResult.Finished;
        }

        public static ActionResult DisableScript(ScriptAction action, ScriptExecutionContext context)
        {
            var scriptName = action.Arguments[0].StringValue;
            context.Scripting.DisableScript(scriptName);
            return ActionResult.Finished;
        }
    }
}

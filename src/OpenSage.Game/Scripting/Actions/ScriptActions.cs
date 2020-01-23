using OpenSage.Data.Map;

namespace OpenSage.Scripting.Actions
{
    public static class ScriptActions
    {
        public static ActionResult EnableScript(ScriptAction action, ScriptExecutionContext context)
        {
            var scriptName = action.Arguments[0].StringValue;
            var script = context.Scripting.FindScript(scriptName);
            if (script != null)
            {
                script.IsActive = true;
            }

            return ActionResult.Finished;
        }

        public static ActionResult DisableScript(ScriptAction action, ScriptExecutionContext context)
        {
            var scriptName = action.Arguments[0].StringValue;
            var script = context.Scripting.FindScript(scriptName);
            if (script != null)
            {
                script.IsActive = false;
            }

            return ActionResult.Finished;
        }

        public static ActionResult CallSubroutine(ScriptAction action, ScriptExecutionContext context)
        {
            var scriptName = action.Arguments[0].StringValue;
            context.Scripting.FindScript(scriptName)?.ExecuteAsSubroutine(context);
            return ActionResult.Finished;
        }
    }
}

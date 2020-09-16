namespace OpenSage.Scripting.Actions
{
    public static class ScriptActions
    {
        public static void EnableScript(ScriptAction action, ScriptExecutionContext context)
        {
            var scriptName = action.Arguments[0].StringValue;
            var script = context.Scripting.FindScript(scriptName);
            if (script != null)
            {
                script.IsActive = true;
            }
        }

        public static void DisableScript(ScriptAction action, ScriptExecutionContext context)
        {
            var scriptName = action.Arguments[0].StringValue;
            var script = context.Scripting.FindScript(scriptName);
            if (script != null)
            {
                script.IsActive = false;
            }
        }

        public static void CallSubroutine(ScriptAction action, ScriptExecutionContext context)
        {
            var scriptName = action.Arguments[0].StringValue;
            context.Scripting.FindScript(scriptName)?.ExecuteAsSubroutine(context);
        }
    }
}

namespace OpenSage.Scripting
{
    partial class ScriptActions
    {
        [ScriptAction(ScriptActionType.NoOp, "Scripting/Debug/Null operation", "Do nothing")]
        public static void NoOp(ScriptExecutionContext context) { }

        [ScriptAction(ScriptActionType.SetFlag, "Scripting/Flags/Set flag to value", "Set {0} to {1}")]
        public static void SetFlag(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.FlagName)] string flagName, bool flagValue)
        {
            context.Scripting.SetFlagValue(flagName, flagValue);
        }

        [ScriptAction(ScriptActionType.EnableScript, "Scripting/Script/Enable script", "Enable {0}")]
        public static void EnableScript(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.ScriptName)] string scriptName)
        {
            EnableScriptInternal(context, scriptName, true);
        }

        [ScriptAction(ScriptActionType.DisableScript, "Scripting/Script/Disable script", "Disable {0}")]
        public static void DisableScript(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.ScriptName)] string scriptName)
        {
            EnableScriptInternal(context, scriptName, false);
        }

        private static void EnableScriptInternal(ScriptExecutionContext context, string scriptName, bool enable)
        {
            var script = context.Scripting.FindScript(scriptName);
            if (script != null)
            {
                script.IsActive = enable;
            }
        }

        [ScriptAction(ScriptActionType.CallSubroutine, "Scripting/Script/Run subroutine script", "Run subroutine {0}")]
        public static void CallSubroutine(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.SubroutineName)] string scriptName)
        {
            context.Scripting.FindScript(scriptName)?.ExecuteAsSubroutine(context);
        }
    }
}

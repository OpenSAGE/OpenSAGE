namespace OpenSage.Scripting.Actions
{
    public static class MiscActions
    {
        public static void NoOp(ScriptAction action, ScriptExecutionContext context) { }

        public static void SetFlag(ScriptAction action, ScriptExecutionContext context)
        {
            var flagName = action.Arguments[0].StringValue;
            var flagValue = action.Arguments[1].IntValueAsBool;

            context.Scripting.Flags[flagName] = flagValue;
        }
    }
}

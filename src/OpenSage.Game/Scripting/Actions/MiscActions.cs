using OpenSage.Data.Map;

namespace OpenSage.Scripting.Actions
{
    public static class MiscActions
    {
        public static ActionResult NoOp(ScriptAction action, ScriptExecutionContext context) => ActionResult.Finished;

        public static ActionResult SetFlag(ScriptAction action, ScriptExecutionContext context)
        {
            var flagName = action.Arguments[0].StringValue;
            var flagValue = action.Arguments[1].IntValueAsBool;

            context.Scripting.Flags[flagName] = flagValue;

            return ActionResult.Finished;
        }
    }
}

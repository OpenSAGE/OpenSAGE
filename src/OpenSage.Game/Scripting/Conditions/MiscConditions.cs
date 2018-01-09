using OpenSage.Data.Map;

namespace OpenSage.Scripting.Conditions
{
    public static class MiscConditions
    {
        public static bool True(ScriptCondition condition, ScriptExecutionContext context) => true;
        public static bool False(ScriptCondition condition, ScriptExecutionContext context) => false;

        public static bool Flag(ScriptCondition condition, ScriptExecutionContext context)
        {
            var flagName = condition.Arguments[0].StringValue;
            var comparedFlagValue = condition.Arguments[1].IntValueAsBool;

            if (!context.Scripting.Flags.TryGetValue(flagName, out var flagValue))
            {
                return false;
            }

            return flagValue == comparedFlagValue;
        }
    }
}

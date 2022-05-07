using System;

namespace OpenSage.Scripting
{
    partial class ScriptConditions
    {
        [ScriptCondition(ScriptConditionType.True, "Scripting/True", "Always true")]
        public static bool True(ScriptExecutionContext context) => true;

        [ScriptCondition(ScriptConditionType.False, "Scripting/False", "Always false")]
        public static bool False(ScriptExecutionContext context) => false;

        [ScriptCondition(ScriptConditionType.Flag, "Scripting/Flag compared to value", "{0} is {1}")]
        public static bool Flag(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.FlagName)] string flagName, bool compareValue)
        {
            var flagValue = context.Scripting.GetFlagValue(flagName);

            return flagValue == compareValue;
        }

        [ScriptCondition(ScriptConditionType.TimerExpired, "Scripting/Timer expired", "{0} has expired")]
        public static bool TimerExpired(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.CounterName)] string timerName)
        {
            return context.Scripting.HasTimerExpired(timerName);
        }

        [ScriptCondition(ScriptConditionType.Counter, "Scripting/Counter compared to value", "{0} is {1} {2}")]
        public static bool Counter(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.CounterName)] string counterName, [ScriptArgumentType(ScriptArgumentType.Comparison)] ScriptingComparison comparison, int compareValue)
        {
            return EvaluateComparison(context.Scripting.GetCounterValue(counterName), comparison, compareValue);
        }

        private static bool EvaluateComparison(int a, ScriptingComparison comp, int b)
        {
            return comp switch
            {
                ScriptingComparison.LessThan => a < b,
                ScriptingComparison.LessOrEqual => a <= b,
                ScriptingComparison.EqualTo => a == b,
                ScriptingComparison.GreaterOrEqual => a >= b,
                ScriptingComparison.GreaterThan => a > b,
                ScriptingComparison.NotEqual => a != b,
                _ => throw new ArgumentException($"Invalid operator: {comp}", nameof(comp)),
            };
        }
    }

    public enum ScriptingComparison
    {
        LessThan = 0,
        LessOrEqual = 1,
        EqualTo = 2,
        GreaterOrEqual = 3,
        GreaterThan = 4,
        NotEqual = 5
    }
}

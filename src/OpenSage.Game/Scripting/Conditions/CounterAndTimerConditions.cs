using System;
using OpenSage.Data.Map;

namespace OpenSage.Scripting.Conditions
{
    public static class CounterAndTimerConditions
    {
        public static bool TimerExpired(ScriptCondition condition, ScriptExecutionContext context)
        {
            var timerName = condition.Arguments[0].StringValue;
            return context.Scripting.Timers.IsTimerExpired(timerName);
        }

        public static bool Counter(ScriptCondition condition, ScriptExecutionContext context)
        {
            var counterName = condition.Arguments[0].StringValue;
            var op = (CounterOperator) condition.Arguments[1].IntValue.Value;
            var comparedValue = condition.Arguments[2].IntValue.Value;

            return EvaluateOperator(context.Scripting.Counters[counterName], op, comparedValue);
        }

        private static bool EvaluateOperator(int a, CounterOperator op, int b)
        {
            switch (op)
            {
                case CounterOperator.LessThan: return a < b;
                case CounterOperator.LessOrEqual: return a <= b;
                case CounterOperator.EqualTo: return a == b;
                case CounterOperator.GreaterOrEqual: return a >= b;
                case CounterOperator.GreaterThan: return a > b;
                case CounterOperator.NotEqual: return a != b;
                default: throw new ArgumentException($"Invalid operator: {op}", nameof(op));
            }
        }

        private enum CounterOperator
        {
            LessThan = 0,
            LessOrEqual = 1,
            EqualTo = 2,
            GreaterOrEqual = 3,
            GreaterThan = 4,
            NotEqual = 5
        }
    }
}

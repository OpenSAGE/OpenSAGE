using System;
using OpenSage.Data.Map;

namespace OpenSage.Scripting.Conditions
{
    public abstract class MapScriptCondition
    {
        public abstract bool Evaluate(ScriptExecutionContext context);
    }

    public sealed class FalseCondition : MapScriptCondition
    {
        public override bool Evaluate(ScriptExecutionContext context)
        {
            return false;
        }
    }

    public sealed class TrueCondition : MapScriptCondition
    {
        public override bool Evaluate(ScriptExecutionContext context)
        {
            return true;
        }
    }

    public sealed class FlagCondition : MapScriptCondition
    {
        private readonly string _flagName;
        private readonly bool _flagValue;

        public FlagCondition(ScriptCondition condition)
        {
            _flagName = condition.Arguments[0].StringValue;
            _flagValue = condition.Arguments[1].IntValueAsBool;
        }

        public override bool Evaluate(ScriptExecutionContext context)
        {
            return context.Scripting.Flags.TryGetValue(_flagName, out var flagValue)
                && flagValue == _flagValue;
        }
    }

    public sealed class TimerExpiredCondition : MapScriptCondition
    {
        private readonly string _timerName;

        public TimerExpiredCondition(ScriptCondition condition)
        {
            _timerName = condition.Arguments[0].StringValue;
        }

        public override bool Evaluate(ScriptExecutionContext context)
        {
            return context.Scripting.Timers.TryGetValue(_timerName, out var timer)
                && timer.Expired;
        }
    }

    public sealed class CompareCounterCondition : MapScriptCondition
    {
        private readonly string _counterName;
        private readonly CounterOperator _operator;
        private readonly int _comparedValue;

        public CompareCounterCondition(ScriptCondition condition)
        {
            _counterName = condition.Arguments[0].StringValue;
            _operator = (CounterOperator) condition.Arguments[1].IntValue.Value;
            _comparedValue = condition.Arguments[2].IntValue.Value;
        }

        public override bool Evaluate(ScriptExecutionContext context)
        {
            if (!context.Scripting.Counters.TryGetValue(_counterName, out var counterValue))
            {
                return false;
            }
            return EvaluateOperator(counterValue, _comparedValue);
        }

        private bool EvaluateOperator(int a, int b)
        {
            switch (_operator)
            {
                case CounterOperator.EqualTo: return a == b;
                default: throw new NotImplementedException(_operator.ToString());
            }
        }

        // TODO: Handle other operators
        private enum CounterOperator
        {
            EqualTo = 2
        }
    }
}

using System;
using System.Diagnostics;
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
            return context.Scripting.Counters.TryGetValue(_counterName, out var counterValue) &&
                   EvaluateOperator(counterValue);
        }

        private bool EvaluateOperator(int x)
        {
            switch (_operator)
            {
                case CounterOperator.LessThan: return x < _comparedValue;
                case CounterOperator.LessOrEqual: return x <= _comparedValue;
                case CounterOperator.EqualTo: return x == _comparedValue;
                case CounterOperator.GreaterOrEqual: return x >= _comparedValue;
                case CounterOperator.GreaterThan: return x > _comparedValue;
                case CounterOperator.NotEqual: return x != _comparedValue;
                default: throw new NotImplementedException(_operator.ToString());
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

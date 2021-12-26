namespace OpenSage.Scripting
{
    partial class ScriptActions
    {
        [ScriptAction(ScriptActionType.IncrementCounter, "Scripting/Counters/Increment counter", "Add {1} to counter {0}")]
        public static void IncrementCounter(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.CounterName)] string counterName, int incrementBy)
        {
            context.Scripting.AddCounterValue(counterName, incrementBy);
        }

        [ScriptAction(ScriptActionType.DecrementCounter, "Scripting/Counters/Decrement counter", "Subtract {1} from counter {0}")]
        public static void DecrementCounter(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.CounterName)] string counterName, int decrementBy)
        {
            context.Scripting.SubtractCounterValue(counterName, decrementBy);
        }

        [ScriptAction(ScriptActionType.SetCounter, "Scripting/Counters/Set counter to value", "Set counter {0} to value {1}")]
        public static void SetCounter(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.CounterName)] string counterName, int value)
        {
            context.Scripting.SetCounterValue(counterName, value);
        }

        [ScriptAction(ScriptActionType.SetTimer, "Scripting/Timers/Set frame countdown timer", "Set timer {0} to expire in {1} frames")]
        public static void SetTimer(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.CounterName)] string timerName, int frames)
        {
            SetFrameTimerInternal(timerName, frames, context);
        }

        [ScriptAction(ScriptActionType.SetMillisecondTimer, "Scripting/Timers/Set seconds countdown timer", "Set timer {0} to expire in {1} seconds")]
        public static void SetMillisecondTimer(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.CounterName)] string timerName, float duration)
        {
            SetFrameTimerInternal(timerName, (int) (duration * context.Scripting.TickRate), context);
        }

        private static void SetFrameTimerInternal(string timerName, int durationFrames, ScriptExecutionContext context)
        {
            context.Scripting.SetTimerValue(timerName, durationFrames);
        }
    }
}

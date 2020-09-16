namespace OpenSage.Scripting.Actions
{
    public static class CounterAndTimerActions
    {
        public static void IncrementCounter(ScriptAction action, ScriptExecutionContext context)
        {
            var counter = action.Arguments[0].StringValue;
            var incrementBy = action.Arguments[1].IntValue.Value;

            context.Scripting.Counters[counter] += incrementBy;
        }

        public static void DecrementCounter(ScriptAction action, ScriptExecutionContext context)
        {
            var counter = action.Arguments[0].StringValue;
            var incrementBy = action.Arguments[1].IntValue.Value;

            context.Scripting.Counters[counter] -= incrementBy;
        }

        public static void SetCounter(ScriptAction action, ScriptExecutionContext context)
        {
            var counter = action.Arguments[0].StringValue;
            var counterValue = action.Arguments[1].IntValue.Value;

            context.Scripting.Counters[counter] = counterValue;
        }

        public static void SetFrameTimer(ScriptAction action, ScriptExecutionContext context)
        {
            var timerName = action.Arguments[0].StringValue;
            var duration = action.Arguments[1].IntValue.Value;

            SetFrameTimerInternal(timerName, duration, context);
        }

        public static void SetMillisecondTimer(ScriptAction action, ScriptExecutionContext context)
        {
            var timerName = action.Arguments[0].StringValue;
            var duration = action.Arguments[1].FloatValue.Value;

            SetFrameTimerInternal(timerName, (int) (duration * ScriptingSystem.TickRate), context);
        }

        private static void SetFrameTimerInternal(string timerName, int durationFrames, ScriptExecutionContext context)
        {
            context.Scripting.Counters[timerName] = durationFrames;
            context.Scripting.Timers.StartTimer(timerName);
        }
    }
}

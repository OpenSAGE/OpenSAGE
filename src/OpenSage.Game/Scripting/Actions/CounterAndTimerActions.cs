using OpenSage.Data.Map;

namespace OpenSage.Scripting.Actions
{
    public static class CounterAndTimerActions
    {
        public static ActionResult IncrementCounter(ScriptAction action, ScriptExecutionContext context)
        {
            var counter = action.Arguments[0].StringValue;
            var incrementBy = action.Arguments[1].IntValue.Value;

            context.Scripting.Counters[counter] += incrementBy;

            return ActionResult.Finished;
        }

        public static ActionResult DecrementCounter(ScriptAction action, ScriptExecutionContext context)
        {
            var counter = action.Arguments[0].StringValue;
            var incrementBy = action.Arguments[1].IntValue.Value;

            context.Scripting.Counters[counter] -= incrementBy;

            return ActionResult.Finished;
        }

        public static ActionResult SetCounter(ScriptAction action, ScriptExecutionContext context)
        {
            var counter = action.Arguments[0].StringValue;
            var counterValue = action.Arguments[1].IntValue.Value;

            context.Scripting.Counters[counter] = counterValue;

            return ActionResult.Finished;
        }

        public static ActionResult SetFrameTimer(ScriptAction action, ScriptExecutionContext context)
        {
            var timerName = action.Arguments[0].StringValue;
            var duration = action.Arguments[1].IntValue.Value;

            return SetFrameTimerInternal(timerName, duration, context);
        }

        public static ActionResult SetMillisecondTimer(ScriptAction action, ScriptExecutionContext context)
        {
            var timerName = action.Arguments[0].StringValue;
            var duration = action.Arguments[1].FloatValue.Value;

            return SetFrameTimerInternal(timerName, (int) (duration * ScriptingSystem.TickRate), context);
        }

        private static ActionResult SetFrameTimerInternal(string timerName, int durationFrames, ScriptExecutionContext context)
        {
            context.Scripting.Counters[timerName] = durationFrames;
            context.Scripting.Timers.StartTimer(timerName);

            return ActionResult.Finished;
        }
    }
}

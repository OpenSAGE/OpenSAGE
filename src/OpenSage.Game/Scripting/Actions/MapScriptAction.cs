using System;
using System.Numerics;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public abstract class MapScriptAction
    {
        public abstract ScriptExecutionResult Execute(ScriptExecutionContext context);

        public virtual void Reset() { }

        protected float CalculateCurrentTimeFraction(
            ScriptExecutionContext context, 
            TimeSpan duration,
            TimeSpan startTime)
        {
            var currentTimeFraction = (float) ((context.UpdateTime.TotalGameTime - startTime).TotalSeconds / duration.TotalSeconds);
            return Math.Min(currentTimeFraction, 1);
        }
    }

    public sealed class SetFlagAction : MapScriptAction
    {
        private readonly string _flagName;
        private readonly bool _flagValue;

        public SetFlagAction(ScriptAction action)
        {
            _flagName = action.Arguments[0].StringValue;
            _flagValue = action.Arguments[1].IntValueAsBool;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scripting.Flags[_flagName] = _flagValue;

            return ScriptExecutionResult.Finished;
        }
    }

    public sealed class SetCounterAction : MapScriptAction
    {
        private readonly string _counterName;
        private readonly int _counterValue;

        public SetCounterAction(ScriptAction action)
        {
            _counterName = action.Arguments[0].StringValue;
            _counterValue = action.Arguments[1].IntValue.Value;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scripting.Counters[_counterName] = _counterValue;

            return ScriptExecutionResult.Finished;
        }
    }

    public sealed class IncrementCounterAction : MapScriptAction
    {
        private readonly string _counterName;
        private readonly int _delta;

        public IncrementCounterAction(ScriptAction action)
        {
            _delta = action.Arguments[0].IntValue.Value;
            _counterName = action.Arguments[1].StringValue;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scripting.Counters[_counterName] += _delta;

            return ScriptExecutionResult.Finished;
        }
    }

    public sealed class DecrementCounterAction : MapScriptAction
    {
        private readonly string _counterName;
        private readonly int _delta;

        public DecrementCounterAction(ScriptAction action)
        {
            _delta = action.Arguments[0].IntValue.Value;
            _counterName = action.Arguments[1].StringValue;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scripting.Counters[_counterName] -= _delta;

            return ScriptExecutionResult.Finished;
        }
    }

    public sealed class NoOpAction : MapScriptAction
    {
        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            return ScriptExecutionResult.Finished;
        }
    }

    public sealed class SetMillisecondTimerAction : MapScriptAction
    {
        private readonly string _timerName;
        private readonly TimeSpan _duration;

        public SetMillisecondTimerAction(ScriptAction action)
        {
            _timerName = action.Arguments[0].StringValue;
            _duration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scripting.Timers[_timerName] = new ScriptTimer(
                context.UpdateTime.TotalGameTime + _duration);

            return ScriptExecutionResult.Finished;
        }
    }

    public sealed class SetupCameraAction : MapScriptAction
    {
        private readonly Waypoint _positionWaypoint;
        private readonly float _zoom;
        private readonly float _pitch;
        private readonly Waypoint _targetWaypoint;

        public SetupCameraAction(ScriptAction action, SceneSettings sceneSettings)
        {
            _positionWaypoint = sceneSettings.Waypoints[action.Arguments[0].StringValue];
            _zoom = action.Arguments[1].FloatValue.Value;
            _pitch = action.Arguments[2].FloatValue.Value;
            _targetWaypoint = sceneSettings.Waypoints[action.Arguments[3].StringValue];
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scene.MainCamera.WorldPosition = _positionWaypoint.Position;
            context.Scene.MainCamera.Zoom = _zoom;
            context.Scene.MainCamera.Pitch = _pitch;
            context.Scene.MainCamera.LookDirection = Vector3.Normalize(_targetWaypoint.Position - _positionWaypoint.Position);

            return ScriptExecutionResult.Finished;
        }
    }

    public enum ScriptExecutionResult
    {
        NotFinished,
        Finished
    }
}

using System;
using System.Numerics;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class MoveCameraToAction : MoveCameraAction
    {
        private readonly Waypoint _targetWaypoint;
        private readonly float _shutter;

        private enum MoveCameraState
        {
            NotStarted,
            Moving
        }

        private MoveCameraState _state;

        private Vector3 _startPosition;
        private Vector3 _direction;

        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public override TimeSpan Duration { get; }
        public override Vector3 EndPosition { get; }

        public MoveCameraToAction(ScriptAction action, SceneSettings sceneSettings)
        {
            _targetWaypoint = sceneSettings.Waypoints[action.Arguments[0].StringValue];

            Duration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);

            // TODO: What is this?
            _shutter = action.Arguments[2].FloatValue.Value;

            EndPosition = _targetWaypoint.Position;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            switch (_state)
            {
                case MoveCameraState.NotStarted:
                    _startPosition = context.Scene.MainCamera.Transform.WorldPosition;
                    _direction = _targetWaypoint.Position - _startPosition;
                    _startTime = context.UpdateTime.TotalGameTime;
                    _endTime = _startTime + Duration;
                    _state = MoveCameraState.Moving;
                    break;
            }

            var currentTimeFraction = CalculateCurrentTimeFraction(
                context, 
                Duration,
                _startTime);

            var currentPosition = _startPosition + _direction * currentTimeFraction;

            context.Scene.MainCamera.WorldPosition = currentPosition;

            return (context.UpdateTime.TotalGameTime >= _endTime)
                ? ScriptExecutionResult.Finished
                : ScriptExecutionResult.NotFinished;
        }

        public override void Reset()
        {
            _state = MoveCameraState.NotStarted;
            _startTime = _endTime = TimeSpan.MinValue;
            _startPosition = _direction = Vector3.Zero;
        }
    }
}

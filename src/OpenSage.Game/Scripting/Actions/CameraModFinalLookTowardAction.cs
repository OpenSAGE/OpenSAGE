using System;
using System.Numerics;
using OpenSage.Mathematics;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class CameraModFinalLookTowardAction : MapScriptAction
    {
        private readonly Waypoint _targetWaypoint;
        private readonly MoveCameraAction _modificationOf;

        private enum State
        {
            NotStarted,
            Moving
        }

        private State _state;

        private Vector3 _startDirection;
        private Vector3 _endDirection;

        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public CameraModFinalLookTowardAction(ScriptAction action, SceneSettings sceneSettings, MoveCameraAction modificationOf)
        {
            _targetWaypoint = sceneSettings.Waypoints[action.Arguments[0].StringValue];
            _modificationOf = modificationOf;

            _endDirection = Vector3.Normalize(_targetWaypoint.Position - _modificationOf.EndPosition);
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            switch (_state)
            {
                case State.NotStarted:
                    _startDirection = context.Scene.CameraController.LookDirection;
                    _startDirection = Vector3.Normalize(_startDirection);
                    _startTime = context.UpdateTime.TotalGameTime;
                    _endTime = _startTime + _modificationOf.Duration;
                    _state = State.Moving;
                    break;
            }

            var currentTimeFraction = CalculateCurrentTimeFraction(context, _modificationOf.Duration, _startTime);

            var direction = Vector3.Normalize(Vector3Utility.Slerp(ref _startDirection, ref _endDirection, currentTimeFraction));

            context.Scene.CameraController.LookDirection = direction;

            return (context.UpdateTime.TotalGameTime >= _endTime)
                ? ScriptExecutionResult.Finished
                : ScriptExecutionResult.NotFinished;
        }

        public override void Reset()
        {
            _state = State.NotStarted;
            _startTime = _endTime = TimeSpan.MinValue;
        }
    }
}

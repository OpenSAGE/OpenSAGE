using System;
using System.Numerics;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public abstract class MoveCameraAction : MapScriptAction
    {
        public abstract TimeSpan Duration { get; }
        public abstract Vector3 EndPosition { get; }
    }

    public sealed class MoveCameraAlongWaypointPathAction : MoveCameraAction
    {
        private readonly WaypointPath _waypointPath;
        private readonly Vector3 _direction;
        private readonly float _shutter;

        private enum MoveCameraState
        {
            NotStarted,
            Moving
        }

        private MoveCameraState _state;

        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public override TimeSpan Duration { get; }
        public override Vector3 EndPosition { get; }

        public MoveCameraAlongWaypointPathAction(ScriptAction action, SceneSettings sceneSettings)
        {
            _waypointPath = sceneSettings.WaypointPaths[action.Arguments[0].StringValue];

            _direction = _waypointPath.End.Position - _waypointPath.Start.Position;

            Duration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);

            // TODO: What is this?
            _shutter = action.Arguments[2].FloatValue.Value;

            EndPosition = _waypointPath.End.Position;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            switch (_state)
            {
                case MoveCameraState.NotStarted:
                    _startTime = context.UpdateTime.TotalGameTime;
                    _endTime = _startTime + Duration;
                    _state = MoveCameraState.Moving;
                    break;
            }

            var currentTimeFraction = CalculateCurrentTimeFraction(
                context, 
                Duration,
                _startTime);

            var currentPosition = _waypointPath.Start.Position + _direction * currentTimeFraction;

            context.Scene.CameraController.TerrainPosition = currentPosition;

            return (context.UpdateTime.TotalGameTime >= _endTime)
                ? ScriptExecutionResult.Finished
                : ScriptExecutionResult.NotFinished;
        }

        public override void Reset()
        {
            _state = MoveCameraState.NotStarted;
            _startTime = _endTime = TimeSpan.MinValue;
        }
    }
}

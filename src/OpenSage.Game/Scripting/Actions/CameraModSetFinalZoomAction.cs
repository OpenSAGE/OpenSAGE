using System;
using System.Numerics;
using OpenSage.Mathematics;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class CameraModSetFinalZoomAction : MapScriptAction
    {
        private readonly float _finalZoom;
        private readonly MoveCameraAction _modificationOf;

        private enum State
        {
            NotStarted,
            Moving
        }

        private State _state;

        private float _startZoom;

        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public CameraModSetFinalZoomAction(ScriptAction action, SceneSettings sceneSettings, MoveCameraAction modificationOf)
        {
            _finalZoom = action.Arguments[0].FloatValue.Value;
            _modificationOf = modificationOf;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            switch (_state)
            {
                case State.NotStarted:
                    _startZoom = context.Scene.CameraController.Zoom;
                    _startTime = context.UpdateTime.TotalGameTime;
                    _endTime = _startTime + _modificationOf.Duration;
                    _state = State.Moving;
                    break;
            }

            var currentTimeFraction = CalculateCurrentTimeFraction(context, _modificationOf.Duration, _startTime);

            var zoom = MathUtility.Lerp(_startZoom, _finalZoom, currentTimeFraction);

            context.Scene.CameraController.Zoom = zoom;

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

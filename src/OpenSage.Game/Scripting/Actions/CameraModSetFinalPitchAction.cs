using System;
using OpenSage.Mathematics;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class CameraModSetFinalPitchAction : MapScriptAction
    {
        private readonly float _finalPitch;
        private readonly MoveCameraAction _modificationOf;

        private enum State
        {
            NotStarted,
            Moving
        }

        private State _state;

        private float _startPitch;

        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public CameraModSetFinalPitchAction(ScriptAction action, SceneSettings sceneSettings, MoveCameraAction modificationOf)
        {
            _finalPitch = action.Arguments[0].FloatValue.Value;
            _modificationOf = modificationOf;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            switch (_state)
            {
                case State.NotStarted:
                    _startPitch = context.Scene.CameraController.Pitch;
                    _startTime = context.UpdateTime.TotalGameTime;
                    _endTime = _startTime + _modificationOf.Duration;
                    _state = State.Moving;
                    break;
            }

            var currentTimeFraction = CalculateCurrentTimeFraction(context, _modificationOf.Duration, _startTime);

            var pitch = MathUtility.Lerp(_startPitch, _finalPitch, currentTimeFraction);

            context.Scene.CameraController.Pitch = pitch;

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

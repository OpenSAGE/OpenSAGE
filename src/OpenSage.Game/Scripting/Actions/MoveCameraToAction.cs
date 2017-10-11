using System;
using OpenSage.Graphics.Cameras;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class MoveCameraToAction : MapScriptAction
    {
        private readonly Waypoint _targetWaypoint;
        private readonly TimeSpan _duration;
        private readonly float _shutter;

        private CameraAnimation _animation;

        public MoveCameraToAction(ScriptAction action, SceneSettings sceneSettings)
        {
            _targetWaypoint = sceneSettings.Waypoints[action.Arguments[0].StringValue];

            _duration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);

            // TODO: What is this?
            _shutter = action.Arguments[2].FloatValue.Value;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            if (_animation == null)
            {
                _animation = context.Scene.CameraController.StartAnimation(
                    context.Scene.Camera.View.Translation,
                    _targetWaypoint.Position,
                    context.UpdateTime.TotalGameTime,
                    _duration);
            }

            return _animation.Finished
                ? ScriptExecutionResult.Finished
                : ScriptExecutionResult.NotFinished;
        }

        public override void Reset()
        {
            _animation = null;
        }
    }
}

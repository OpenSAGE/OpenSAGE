using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Graphics.Cameras;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class MoveCameraAlongWaypointPathAction : MapScriptAction
    {
        private readonly TimeSpan _totalDuration;
        private readonly float _shutter;
        private readonly List<Waypoint> _path;

        private CameraAnimation _animation;
        private int _currentNode;

        public MoveCameraAlongWaypointPathAction(ScriptAction action, SceneSettings sceneSettings)
        {
            var firstNode = sceneSettings.Waypoints[action.Arguments[0].StringValue];
            _path = sceneSettings.WaypointPaths.GetFullPath(firstNode).ToList();
            _currentNode = 0;

            _totalDuration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);

            // TODO: What is this?
            _shutter = action.Arguments[2].FloatValue.Value;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            if (_animation != null && !_animation.Finished)
            {
                return ScriptExecutionResult.NotFinished;
            }

            if (_currentNode >= _path.Count - 1)
            {
                return ScriptExecutionResult.Finished;
            }

            var start = _path[_currentNode];
            var end = _path[++_currentNode];

            // TODO: Test if this is the right "algorithm", or if the animation duration is relative to the distance between nodes.
            var duration = new TimeSpan(_totalDuration.Ticks / _path.Count - 1);

            _animation = context.Scene.CameraController.StartAnimation(
                start.Position,
                end.Position,
                context.UpdateTime.TotalGameTime,
                duration);

            return ScriptExecutionResult.NotFinished;
        }

        private bool ContinueAnimating => (_animation == null || _animation.Finished) && _currentNode < _path.Count - 1;

        public override void Reset()
        {
            _animation = null;
            _currentNode = 0;
        }
    }
}

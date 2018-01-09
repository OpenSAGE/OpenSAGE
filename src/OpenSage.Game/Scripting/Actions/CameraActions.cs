using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Settings;
using CameraAnimation = OpenSage.Graphics.Cameras.CameraAnimation;

namespace OpenSage.Scripting.Actions
{
    public static class CameraActions
    {
        public static ActionResult SetupCamera(ScriptAction action, ScriptExecutionContext context)
        {
            var positionWaypoint = context.Scene.Settings.Waypoints[action.Arguments[0].StringValue];
            var zoom = action.Arguments[1].FloatValue.Value;
            var pitch = action.Arguments[2].FloatValue.Value;
            var targetWaypoint = context.Scene.Settings.Waypoints[action.Arguments[3].StringValue];

            context.Scene.CameraController.EndAnimation();

            context.Scene.CameraController.TerrainPosition = positionWaypoint.Position;
            context.Scene.CameraController.Zoom = zoom;
            context.Scene.CameraController.Pitch = pitch;
            context.Scene.CameraController.SetLookDirection(Vector3.Normalize(targetWaypoint.Position - positionWaypoint.Position));

            return ActionResult.Finished;
        }

        public static ActionResult CameraModSetFinalZoom(ScriptAction action, ScriptExecutionContext context)
        {
            var finalZoom = action.Arguments[0].FloatValue.Value;

            context.Scene.CameraController.ModSetFinalZoom(finalZoom);

            return ActionResult.Finished;
        }

        public static ActionResult CameraModSetFinalPitch(ScriptAction action, ScriptExecutionContext context)
        {
            var finalPitch = action.Arguments[0].FloatValue.Value;

            context.Scene.CameraController.ModSetFinalPitch(finalPitch);

            return ActionResult.Finished;
        }

        public static ActionResult CameraModFinalLookToward(ScriptAction action, ScriptExecutionContext context)
        {
            var waypointName = action.Arguments[0].StringValue;
            var waypoint = context.Scene.Settings.Waypoints[waypointName];

            context.Scene.CameraController.ModFinalLookToward(waypoint.Position);

            return ActionResult.Finished;
        }

        public static ActionResult MoveCameraTo(ScriptAction action, ScriptExecutionContext context)
        {
            var targetWaypoint = context.Scene.Settings.Waypoints[action.Arguments[0].StringValue];
            var duration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);
            var shutter = action.Arguments[2].FloatValue.Value;

            return new MoveCameraToAction(targetWaypoint, duration, shutter).Execute(context);
        }

        public static ActionResult MoveCameraAlongWaypointPath(ScriptAction action, ScriptExecutionContext context)
        {
            var firstNode = context.Scene.Settings.Waypoints[action.Arguments[0].StringValue];
            var path = context.Scene.Settings.WaypointPaths.GetFullPath(firstNode).ToList();
            var totalDuration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);
            var shutter = action.Arguments[2].FloatValue.Value;

            return new MoveCameraAlongWaypointPathAction(path, totalDuration, shutter).Execute(context);
        }
    }

    public sealed class MoveCameraToAction : ActionResult.ActionContinuation
    {
        private readonly Waypoint _targetWaypoint;
        private readonly TimeSpan _duration;
        private readonly float _shutter;

        private CameraAnimation _animation;

        public MoveCameraToAction(Waypoint targetWaypoint, TimeSpan duration, float shutter)
        {
            _shutter = shutter;
            _duration = duration;
            _targetWaypoint = targetWaypoint;
        }

        public override ActionResult Execute(ScriptExecutionContext context)
        {
            if (_animation == null)
            {
                _animation = context.Scene.CameraController.StartAnimation(
                    context.Scene.Camera.View.Translation,
                    _targetWaypoint.Position,
                    context.UpdateTime.TotalGameTime,
                    _duration);
            }

            return _animation.Finished ? ActionResult.Finished : this;
        }
    }

    public sealed class MoveCameraAlongWaypointPathAction : ActionResult.ActionContinuation
    {
        private readonly List<Waypoint> _path;
        private readonly float _shutter;
        private readonly TimeSpan _totalDuration;

        private CameraAnimation _animation;
        private int _currentNode;

        public MoveCameraAlongWaypointPathAction(List<Waypoint> path, TimeSpan totalDuration, float shutter)
        {
            _path = path;
            _totalDuration = totalDuration;
            // TODO: What is this?
            _shutter = shutter;
        }

        public override ActionResult Execute(ScriptExecutionContext context)
        {
            if (_animation != null && !_animation.Finished) return this;

            if (_currentNode >= _path.Count - 1) return ActionResult.Finished;

            var start = _path[_currentNode];
            var end = _path[++_currentNode];

            // TODO: Test if this is the right "algorithm", or if the animation duration is relative to the distance between nodes.
            var duration = new TimeSpan(_totalDuration.Ticks / _path.Count - 1);

            // TODO: StartAnimation should probably take the entire path at once, so it can perform interpolation.
            _animation = context.Scene.CameraController.StartAnimation(
                start.Position,
                end.Position,
                context.UpdateTime.TotalGameTime,
                duration);

            return this;
        }
    }
}

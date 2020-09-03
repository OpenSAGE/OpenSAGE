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
            var positionWaypoint = context.Scene.Waypoints[action.Arguments[0].StringValue];
            var zoom = action.Arguments[1].FloatValue.Value;
            var pitch = action.Arguments[2].FloatValue.Value;
            var targetWaypoint = context.Scene.Waypoints[action.Arguments[3].StringValue];

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
            var waypoint = context.Scene.Waypoints[waypointName];

            context.Scene.CameraController.ModFinalLookToward(waypoint.Position);

            return ActionResult.Finished;
        }


        public static ActionResult CameraModLookToward(ScriptAction action, ScriptExecutionContext context)
        {
            var waypointName = action.Arguments[0].StringValue;
            var waypoint = context.Scene.Waypoints[waypointName];

            context.Scene.CameraController.ModLookToward(waypoint.Position);

            return ActionResult.Finished;
        }

        public static ActionResult MoveCameraTo(ScriptAction action, ScriptExecutionContext context)
        {
            //Check if a named camera exists
            Vector3 targetPoint;
            var name = action.Arguments[0].StringValue;
            if (context.Scene.Cameras.Exists(name))
            {
                targetPoint = context.Scene.Cameras[name].LookAtPoint;
                // TODO: Zoom
                // TODO: EulerAngles
            }
            else
            {
                targetPoint = context.Scene.Waypoints[name].Position;
            }

            var duration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);
            var shutter = action.Arguments[2].FloatValue.Value;

            return new MoveCameraToAction(targetPoint, duration, shutter).Execute(context);
        }

        public static ActionResult MoveCameraAlongWaypointPath(ScriptAction action, ScriptExecutionContext context)
        {
            var firstWaypointName = action.Arguments[0].StringValue;
            if (!context.Scene.Waypoints.TryGetByName(firstWaypointName, out var waypoint))
            {
                ScriptingSystem.Logger.Warn($"Waypoint \"{firstWaypointName}\" does not exist.");
                return ActionResult.Finished;
            }

            var totalDuration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);
            var shutter = action.Arguments[2].FloatValue.Value;

            return new MoveCameraAlongWaypointPathAction(waypoint.FollowPath(context.Scene.Random), totalDuration, shutter).Execute(context);
        }

        public static ActionResult TerrainRenderDisable(ScriptAction action, ScriptExecutionContext context)
        {
            var disable = action.Arguments[0].IntValueAsBool;

            context.Scene.ShowTerrain = !disable;

            return ActionResult.Finished;
        }
    }

    public sealed class MoveCameraToAction : ActionResult.ActionContinuation
    {
        private readonly Vector3 _targetPoint;
        private readonly TimeSpan _duration;
        private readonly float _shutter;

        private CameraAnimation _animation;

        public MoveCameraToAction(Vector3 targetPoint, TimeSpan duration, float shutter)
        {
            _shutter = shutter;
            _duration = duration;
            _targetPoint = targetPoint;
        }

        public override ActionResult Execute(ScriptExecutionContext context)
        {
            if (_animation == null)
            {
                _animation = context.Scene.CameraController.StartAnimation(
                    new[] { context.Scene.CameraController.TerrainPosition, _targetPoint },
                    context.UpdateTime.TotalTime,
                    _duration);
            }

            return _animation.Finished ? Finished : this;
        }
    }

    public sealed class MoveCameraAlongWaypointPathAction : ActionResult.ActionContinuation
    {
        private readonly IEnumerable<Vector3> _path;
        private readonly float _shutter;
        private readonly TimeSpan _totalDuration;

        private CameraAnimation _animation;

        public MoveCameraAlongWaypointPathAction(IEnumerable<Vector3> path, TimeSpan totalDuration, float shutter)
        {
            _path = path;
            _totalDuration = totalDuration;
            // TODO: What is this?
            _shutter = shutter;
        }

        public override ActionResult Execute(ScriptExecutionContext context)
        {
            if (_animation == null)
            {
                // TODO: Avoid allocating this list?
                // TODO: Does the real engine start the animation from the current position?

                // Calling .ToList() here is dangerous, as a path could contain a loop,
                // but it shouldn't happen for camera paths (the original ZH freezes as well
                // when you try this).
                var pathWithCurrentPos = _path.Prepend(context.Scene.CameraController.TerrainPosition).ToList();

                _animation = context.Scene.CameraController.StartAnimation(
                    pathWithCurrentPos,
                    context.UpdateTime.TotalTime, _totalDuration);
            }

            return _animation.Finished ? ActionResult.Finished : this;
        }
    }
}

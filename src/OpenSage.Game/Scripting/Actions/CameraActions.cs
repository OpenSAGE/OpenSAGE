using System;
using System.Linq;
using System.Numerics;

namespace OpenSage.Scripting.Actions
{
    public static class CameraActions
    {
        public static void SetupCamera(ScriptAction action, ScriptExecutionContext context)
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
        }

        public static void CameraModSetFinalZoom(ScriptAction action, ScriptExecutionContext context)
        {
            var finalZoom = action.Arguments[0].FloatValue.Value;

            context.Scene.CameraController.ModSetFinalZoom(finalZoom);
        }

        public static void CameraModSetFinalPitch(ScriptAction action, ScriptExecutionContext context)
        {
            var finalPitch = action.Arguments[0].FloatValue.Value;

            context.Scene.CameraController.ModSetFinalPitch(finalPitch);
        }

        public static void CameraModFinalLookToward(ScriptAction action, ScriptExecutionContext context)
        {
            var waypointName = action.Arguments[0].StringValue;
            var waypoint = context.Scene.Waypoints[waypointName];

            context.Scene.CameraController.ModFinalLookToward(waypoint.Position);
        }


        public static void CameraModLookToward(ScriptAction action, ScriptExecutionContext context)
        {
            var waypointName = action.Arguments[0].StringValue;
            var waypoint = context.Scene.Waypoints[waypointName];

            context.Scene.CameraController.ModLookToward(waypoint.Position);
        }

        public static void MoveCameraTo(ScriptAction action, ScriptExecutionContext context)
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

            context.Scene.CameraController.StartAnimation(
                new[] { context.Scene.CameraController.TerrainPosition, targetPoint },
                context.UpdateTime.TotalTime,
                duration);
        }

        public static void MoveCameraAlongWaypointPath(ScriptAction action, ScriptExecutionContext context)
        {
            var firstWaypointName = action.Arguments[0].StringValue;
            if (!context.Scene.Waypoints.TryGetByName(firstWaypointName, out var waypoint))
            {
                ScriptingSystem.Logger.Warn($"Waypoint \"{firstWaypointName}\" does not exist.");
                return;
            }

            var totalDuration = TimeSpan.FromSeconds(action.Arguments[1].FloatValue.Value);
            var shutter = action.Arguments[2].FloatValue.Value;

            var path = waypoint.FollowPath(context.Scene.Random);

            // TODO: Avoid allocating this list?
            // TODO: Does the real engine start the animation from the current position?

            // Calling .ToList() here is dangerous, as a path could contain a loop,
            // but it shouldn't happen for camera paths (the original ZH freezes as well
            // when you try this).
            var pathWithCurrentPos = path.Prepend(context.Scene.CameraController.TerrainPosition).ToList();

            context.Scene.CameraController.StartAnimation(
                pathWithCurrentPos,
                context.UpdateTime.TotalTime, totalDuration);
        }

        public static void TerrainRenderDisable(ScriptAction action, ScriptExecutionContext context)
        {
            var disable = action.Arguments[0].IntValueAsBool;

            context.Scene.ShowTerrain = !disable;
        }
    }
}

using System;
using System.Linq;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Scripting
{
    partial class ScriptActions
    {
        [ScriptAction(ScriptActionType.SetupCamera, "Camera/Adjust/Set up the camera", "Position camera at waypoint {0}, with zoom {1} and pitch {2}, looking towards {3}")]
        public static void SetupCamera(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.WaypointName)] string positionWaypointName, float zoom, float pitch, [ScriptArgumentType(ScriptArgumentType.WaypointName)] string targetWaypointName)
        {
            var positionWaypoint = context.Scene.Waypoints[positionWaypointName];
            var targetWaypoint = context.Scene.Waypoints[targetWaypointName];

            context.Scene.CameraController.EndAnimation();

            context.Scene.CameraController.TerrainPosition = positionWaypoint.Position;
            context.Scene.CameraController.Zoom = zoom;
            context.Scene.CameraController.SetPitch(pitch);
            context.Scene.CameraController.SetLookDirection(Vector3.Normalize(targetWaypoint.Position - positionWaypoint.Position));
        }

        [ScriptAction(ScriptActionType.CameraModSetFinalZoom, "Camera/Move/Set final zoom for camera movement", "Adjust zoom to {0}")]
        public static void CameraModSetFinalZoom(ScriptExecutionContext context, float finalZoom)
        {
            context.Scene.CameraController.ModSetFinalZoom(finalZoom);
        }

        [ScriptAction(ScriptActionType.CameraModSetFinalPitch, "Camera/Move/Set final pitch for camera movement", "Adjust pitch to {0}")]
        public static void CameraModSetFinalPitch(ScriptExecutionContext context, float finalPitch)
        {
            context.Scene.CameraController.ModSetFinalPitch(finalPitch);
        }

        [ScriptAction(ScriptActionType.CameraModFinalLookToward, "Camera/Move/Set final look-toward point for camera movement", "Look toward {0} at the end of the camera movement")]
        public static void CameraModFinalLookToward(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.WaypointName)] string waypointName)
        {
            var waypoint = context.Scene.Waypoints[waypointName];

            context.Scene.CameraController.ModFinalLookToward(waypoint.Position);
        }

        [ScriptAction(ScriptActionType.CameraModLookToward, "Camera/Move/Camera look toward point while moving", "Look toward {0} during the camera movement")]
        public static void CameraModLookToward(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.WaypointName)] string waypointName)
        {
            var waypoint = context.Scene.Waypoints[waypointName];

            context.Scene.CameraController.ModLookToward(waypoint.Position);
        }

        // There are two versions of MoveCameraTo:
        // - In Generals and ZH, the first argument is a waypoint.
        // - In BFME and later, the first argument is a NamedCamera.
        //
        // So we have two methods here. The first specifies via the [ScriptAction]
        // attribute that it is only for Generals and Zero Hour.
        //
        // The second is for all other games.

        [ScriptAction(ScriptActionType.MoveCameraTo, "Camera/Move/Move the camera to a location", "Move camera to {0} in {1} seconds, camera shutter {2} seconds", SageGame.CncGenerals, SageGame.CncGeneralsZeroHour)]
        public static void MoveCameraTo(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.WaypointName)] string waypointName, float rawDuration, float shutter)
        {
            var targetPoint = context.Scene.Waypoints[waypointName].Position;

            var duration = TimeSpan.FromSeconds(rawDuration);

            context.Scene.CameraController.StartAnimation(
                new[] { context.Scene.CameraController.TerrainPosition, targetPoint },
                context.UpdateTime.TotalTime,
                duration);
        }

        [ScriptAction(ScriptActionType.MoveCameraTo, "Camera/Move/Move the camera to a location", "Move camera to {0} in {1} seconds, camera shutter {2} seconds, ease-in {3} seconds, ease-out {4} seconds")]
        public static void MoveCameraTo(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.WaypointName)] string cameraName, float rawDuration, float shutter, float easeIn, float easeOut)
        {
            var camera = context.Scene.Cameras[cameraName];

            var targetPoint = camera.LookAtPoint;

            var duration = TimeSpan.FromSeconds(rawDuration);

            var animation = context.Scene.CameraController.StartAnimation(
                new[] { context.Scene.CameraController.TerrainPosition, targetPoint },
                context.UpdateTime.TotalTime,
                duration);

            var rotation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(camera.Yaw, 0, camera.Roll);
            var lookToward = Vector3.Transform(Vector3.UnitY, rotation);
            animation.SetFinalLookDirection(lookToward);

            animation.SetFinalPitchAngle(-camera.Pitch);

            animation.SetFinalZoom(camera.Zoom);

            animation.SetFinalFieldOfView(camera.FieldOfView);
        }

        [ScriptAction(ScriptActionType.MoveCameraAlongWaypointPath, "Camera/Move/Move camera along a waypoint path", "Move along path starting with {0} in {1} seconds, camera shutter {2} seconds")]
        public static void MoveCameraAlongWaypointPath(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.WaypointName)] string firstWaypointName, float rawTotalDuration, float shutter)
        {
            if (!context.Scene.Waypoints.TryGetByName(firstWaypointName, out var waypoint))
            {
                ScriptingSystem.Logger.Warn($"Waypoint \"{firstWaypointName}\" does not exist.");
                return;
            }

            var totalDuration = TimeSpan.FromSeconds(rawTotalDuration);

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

        [ScriptAction(ScriptActionType.TerrainRenderDisable, "Camera/Terrain/Enable or disable terrain rendering", "Disable terrain rendering {0} (true to disable)")]
        public static void TerrainRenderDisable(ScriptExecutionContext context, bool disable)
        {
            context.Scene.ShowTerrain = !disable;
        }

        [ScriptAction(ScriptActionType.CameraFadeAdd, "Camera/Fade/Fade using an additive blend to white", "Fade (0-1) from {0} to {1} adding towards white. Take {2} frames to increase. Hold for {3} frames. Decrease for {4} frames.")]
        public static void CameraFadeAdd(ScriptExecutionContext context, float from, float to, int framesIncrease, int framesHold, int framesDecrease)
        {
            context.Scripting.SetCameraFade(CameraFadeType.AdditiveBlendToWhite, from, to, (uint)framesIncrease, (uint)framesHold, (uint)framesDecrease);
        }
    }
}

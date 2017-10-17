using System.Collections.Generic;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;
using ScriptActionType = OpenSage.Data.Map.ScriptActionType;

namespace OpenSage.Scripting.Actions
{
    public static class MapScriptActionFactory
    {
        private delegate MapScriptAction CreateMapScriptAction(ScriptAction action, SceneSettings sceneSettings);

        private static readonly Dictionary<ScriptActionType, CreateMapScriptAction> Factories = new Dictionary<ScriptActionType, CreateMapScriptAction>
        {
            { ScriptActionType.SetFlag, (a, s) => new SetFlagAction(a) },
            { ScriptActionType.SetCounter, (a, s) => new SetCounterAction(a) },
            { ScriptActionType.NoOp, (a, s) => new NoOpAction() },
            { ScriptActionType.MoveCameraTo, (a, s) => new MoveCameraToAction(a, s) },
            { ScriptActionType.IncrementCounter, (a, s) => new IncrementCounterAction(a) },
            { ScriptActionType.DecrementCounter, (a, s) => new DecrementCounterAction(a) },
            { ScriptActionType.MoveCameraAlongWaypointPath, (a, s) => new MoveCameraAlongWaypointPathAction(a, s) },
            { ScriptActionType.SetMillisecondTimer, (a, s) => new SetMillisecondTimerAction(a) },
            { ScriptActionType.CameraModSetFinalZoom, (a, s) => new CameraModSetFinalZoomAction(a, s) },
            { ScriptActionType.CameraModSetFinalPitch, (a, s) => new CameraModSetFinalPitchAction(a, s) },
            { ScriptActionType.CameraModFinalLookToward, (a, s) => new CameraModFinalLookTowardAction(a, s) },
            { ScriptActionType.SetupCamera, (a, s) => new SetupCameraAction(a, s) },
            { ScriptActionType.TeamFollowWaypointsExact, (a, s) => new TeamFollowWaypointsExactAction(a, s) }
        };

        public static MapScriptAction Create(ScriptAction action, SceneSettings sceneSettings, List<MapScriptAction> previousActions)
        {
            if (!Factories.TryGetValue(action.ContentType, out var factory))
            {
                // TODO: Implement this action type.
                return new NoOpAction();
            }

            return factory(action, sceneSettings);
        }
    }
}

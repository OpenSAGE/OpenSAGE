using System.Collections.Generic;
using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;
using ScriptActionType = OpenSage.Data.Map.ScriptActionType;

namespace OpenSage.Scripting.Actions
{
    public static class MapScriptActionFactory
    {
        private delegate MapScriptAction CreateMapScriptAction(ScriptAction action, SceneSettings sceneSettings, MapScriptAction modificationOf);

        private static readonly Dictionary<ScriptActionType, CreateMapScriptAction> Factories = new Dictionary<ScriptActionType, CreateMapScriptAction>
        {
            { ScriptActionType.SetFlag, (a, s, p) => new SetFlagAction(a) },
            { ScriptActionType.SetCounter, (a, s, p) => new SetCounterAction(a) },
            { ScriptActionType.NoOp, (a, s, p) => new NoOpAction() },
            { ScriptActionType.MoveCameraTo, (a, s, p) => new MoveCameraToAction(a, s) },
            { ScriptActionType.IncrementCounter, (a, s, p) => new IncrementCounterAction(a) },
            { ScriptActionType.DecrementCounter, (a, s, p) => new DecrementCounterAction(a) },
            { ScriptActionType.MoveCameraAlongWaypointPath, (a, s, p) => new MoveCameraAlongWaypointPathAction(a, s) },
            { ScriptActionType.SetMillisecondTimer, (a, s, p) => new SetMillisecondTimerAction(a) },
            { ScriptActionType.CameraModSetFinalZoom, (a, s, p) => new CameraModSetFinalZoomAction(a, s, (MoveCameraAction) p) },
            { ScriptActionType.CameraModSetFinalPitch, (a, s, p) => new CameraModSetFinalPitchAction(a, s, (MoveCameraAction) p) },
            { ScriptActionType.CameraModFinalLookToward, (a, s, p) => new CameraModFinalLookTowardAction(a, s, (MoveCameraAction) p) },
            { ScriptActionType.SetupCamera, (a, s, p) => new SetupCameraAction(a, s) },
        };

        public static MapScriptAction Create(ScriptAction action, SceneSettings sceneSettings, List<MapScriptAction> previousActions)
        {
            if (!Factories.TryGetValue(action.ContentType, out var factory))
            {
                // TODO: Implement this action type.
                return new NoOpAction();
            }

            MapScriptAction previousAction = null;
            switch (action.ContentType)
            {
                case ScriptActionType.CameraModSetFinalZoom:
                case ScriptActionType.CameraModSetFinalPitch:
                case ScriptActionType.CameraModFinalLookToward:
                    previousAction = previousActions.FindLast(x => x is MoveCameraAction);
                    break;
            }

            return factory(action, sceneSettings, previousAction);
        }
    }
}

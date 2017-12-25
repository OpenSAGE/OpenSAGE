using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class CameraModFinalLookTowardAction : MapScriptAction
    {
        private readonly Waypoint _targetWaypoint;

        public CameraModFinalLookTowardAction(ScriptAction action, SceneSettings sceneSettings)
        {
            _targetWaypoint = sceneSettings.Waypoints[action.Arguments[0].StringValue];
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scene.CameraController.ModFinalLookToward(_targetWaypoint.Position);
            return ScriptExecutionResult.Finished;
        }
    }
}

using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class CameraModSetFinalZoomAction : MapScriptAction
    {
        private readonly float _finalZoom;

        public CameraModSetFinalZoomAction(ScriptAction action, SceneSettings sceneSettings)
        {
            _finalZoom = action.Arguments[0].FloatValue.Value;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scene.CameraController.ModSetFinalZoom(_finalZoom);
            return ScriptExecutionResult.Finished;
        }
    }
}

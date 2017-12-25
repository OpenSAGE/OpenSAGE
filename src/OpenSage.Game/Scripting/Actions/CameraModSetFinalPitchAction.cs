using OpenSage.Settings;
using ScriptAction = OpenSage.Data.Map.ScriptAction;

namespace OpenSage.Scripting.Actions
{
    public sealed class CameraModSetFinalPitchAction : MapScriptAction
    {
        private readonly float _finalPitch;

        public CameraModSetFinalPitchAction(ScriptAction action, SceneSettings sceneSettings)
        {
            _finalPitch = action.Arguments[0].FloatValue.Value;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scene.CameraController.ModSetFinalPitch(_finalPitch);
            return ScriptExecutionResult.Finished;
        }
    }
}

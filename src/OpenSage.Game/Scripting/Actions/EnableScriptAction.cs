using OpenSage.Data.Map;
using OpenSage.Settings;

namespace OpenSage.Scripting.Actions
{
    public sealed class EnableScriptAction : MapScriptAction
    {
        private readonly string _scriptName;

        public EnableScriptAction(ScriptAction action)
        {
            _scriptName = action.Arguments[0].StringValue;
        }

        public override ScriptExecutionResult Execute(ScriptExecutionContext context)
        {
            context.Scripting.RestartScript(_scriptName);
            return ScriptExecutionResult.Finished;
        }
    }
}

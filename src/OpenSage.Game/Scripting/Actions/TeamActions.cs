using OpenSage.Data.Map;

namespace OpenSage.Scripting.Actions
{
    public static class TeamActions
    {
        public static ActionResult TeamFollowWaypointsExact(ScriptAction action, ScriptExecutionContext context)
        {
            var teamName = action.Arguments[0].StringValue;
            var waypointPath = context.Scene.WaypointPaths[action.Arguments[1].StringValue];
            var asTeam = action.Arguments[2].IntValueAsBool;

            // TODO: Implement this.
            return ActionResult.Finished;
        }
    }
}

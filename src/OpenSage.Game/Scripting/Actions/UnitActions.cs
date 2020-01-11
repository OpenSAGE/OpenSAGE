using OpenSage.Data.Map;

namespace OpenSage.Scripting.Actions
{
    public static class UnitActions
    {
        public static ActionResult MoveNamedUnitTo(ScriptAction action, ScriptExecutionContext context)
        {
            var unitName = action.Arguments[0].StringValue;
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return ActionResult.Finished;
            }

            var waypointName = action.Arguments[1].StringValue;
            if (!context.Scene.Waypoints.TryGetByName(waypointName, out var waypoint))
            {
                ScriptingSystem.Logger.Warn($"Waypoint \"{waypointName}\" does not exist.");
                return ActionResult.Finished;
            }

            unit.SetTargetPoint(waypoint.Position);

            return ActionResult.Finished;
        }
    }
}

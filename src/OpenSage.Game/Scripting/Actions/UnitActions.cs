using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Scripting.Actions
{
    public static class UnitActions
    {
        public static ActionResult NamedStop(ScriptAction action, ScriptExecutionContext context)
        {
            var unitName = action.Arguments[0].StringValue;
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return ActionResult.Finished;
            }

            unit.Stop();

            return ActionResult.Finished;
        }

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

        public static ActionResult NamedFollowWaypoints(ScriptAction action, ScriptExecutionContext context)
        {
            var unitName = action.Arguments[0].StringValue;
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return ActionResult.Finished;
            }

            var pathLabel = action.Arguments[1].StringValue;

            var waypoints = context.Scene.Waypoints.GetByPathLabel(pathLabel);
            var nearestWaypoint = waypoints.MinByOrDefault(w => Vector3.DistanceSquared(unit.Transform.Translation, w.Position));
            if (nearestWaypoint != null)
            {
                // A waypoint path can contain branches and loops, so we need:
                // a) a random number generator to pick a when there is more than one and
                // b) a lazily evaluated enumerable of positions
                var positions = nearestWaypoint.FollowPath(context.Game.Scene3D.Random);
                unit.FollowWaypoints(positions);
            }

            return ActionResult.Finished;
        }
    }
}

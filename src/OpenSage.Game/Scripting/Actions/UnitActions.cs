using System.Numerics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Scripting.Actions
{
    public static class UnitActions
    {
        public static void NamedStop(ScriptAction action, ScriptExecutionContext context)
        {
            var unitName = action.Arguments[0].StringValue;
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return;
            }

            unit.AIUpdate.Stop();
        }

        public static void MoveNamedUnitTo(ScriptAction action, ScriptExecutionContext context)
        {
            var unitName = action.Arguments[0].StringValue;
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return;
            }

            var waypointName = action.Arguments[1].StringValue;
            if (!context.Scene.Waypoints.TryGetByName(waypointName, out var waypoint))
            {
                ScriptingSystem.Logger.Warn($"Waypoint \"{waypointName}\" does not exist.");
                return;
            }

            unit.AIUpdate.SetTargetPoint(waypoint.Position);
        }

        public static void NamedFollowWaypoints(ScriptAction action, ScriptExecutionContext context)
        {
            var unitName = action.Arguments[0].StringValue;
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return;
            }

            var pathLabel = action.Arguments[1].StringValue;

            var waypoints = context.Scene.Waypoints.GetByPathLabel(pathLabel);

            // The unit should not necessarily start at the first waypoint in the path, but at the nearest.
            var nearestWaypoint = waypoints.MinByOrDefault(w => Vector3.DistanceSquared(unit.Transform.Translation, w.Position));
            if (nearestWaypoint != null)
            {
                var positions = nearestWaypoint.FollowPath(context.Scene.Random);
                unit.AIUpdate.FollowWaypoints(positions);
            }
        }
    }
}

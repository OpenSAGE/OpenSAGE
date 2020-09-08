using System.Numerics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Scripting
{
    partial class ScriptActions
    {
        [ScriptAction(ScriptActionType.NamedStop, "Unit/Stop/Stop unit", "Stop {0} moving")]
        public static void NamedStop(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.UnitName)] string unitName)
        {
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return;
            }

            unit.AIUpdate.Stop();
        }

        [ScriptAction(ScriptActionType.MoveNamedUnitTo, "Unit/Move/Move unit to a location", "Move {0} to {1}")]
        public static void MoveNamedUnitTo(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.UnitName)] string unitName, [ScriptArgumentType(ScriptArgumentType.WaypointName)] string waypointName)
        {
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return;
            }

            if (!context.Scene.Waypoints.TryGetByName(waypointName, out var waypoint))
            {
                ScriptingSystem.Logger.Warn($"Waypoint \"{waypointName}\" does not exist.");
                return;
            }

            unit.AIUpdate.SetTargetPoint(waypoint.Position);
        }

        [ScriptAction(ScriptActionType.NamedFollowWaypoints, "Unit/Move/Follow a waypoint path", "{0} follows waypoints, beginning at {1}")]
        public static void NamedFollowWaypoints(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.UnitName)] string unitName, [ScriptArgumentType(ScriptArgumentType.WaypointPathName)] string waypointPathName)
        {
            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return;
            }

            var waypoints = context.Scene.Waypoints.GetByPathLabel(waypointPathName);

            // The unit should not necessarily start at the first waypoint in the path, but at the nearest.
            var nearestWaypoint = waypoints.MinByOrDefault(w => Vector3.DistanceSquared(unit.Translation, w.Position));
            if (nearestWaypoint != null)
            {
                var positions = nearestWaypoint.FollowPath(context.Scene.Random);
                unit.AIUpdate.FollowWaypoints(positions);
            }
        }
    }
}

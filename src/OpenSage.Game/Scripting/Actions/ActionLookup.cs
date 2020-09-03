using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using ScriptAction = OpenSage.Data.Map.ScriptAction;
using ScriptActionType = OpenSage.Data.Map.ScriptActionType;

namespace OpenSage.Scripting.Actions
{
    public static class ActionLookup
    {
        // TODO?: This is technically unnecessary boilerplate; we could construct this table dynamically using reflection.
        // If we want even more magic we could even figure out the parameters for each action and pre-parse them.
        private static readonly Dictionary<ScriptActionType, ScriptingAction> Actions = new Dictionary<ScriptActionType, ScriptingAction>
        {
            // Misc
            { ScriptActionType.NoOp, MiscActions.NoOp },
            { ScriptActionType.SetFlag, MiscActions.SetFlag },

            // Counter and timer
            { ScriptActionType.SetCounter, CounterAndTimerActions.SetCounter },
            { ScriptActionType.IncrementCounter, CounterAndTimerActions.IncrementCounter },
            { ScriptActionType.DecrementCounter, CounterAndTimerActions.DecrementCounter },
            { ScriptActionType.SetMillisecondTimer, CounterAndTimerActions.SetMillisecondTimer },
            { ScriptActionType.SetTimer, CounterAndTimerActions.SetFrameTimer },

            // Scripting
            { ScriptActionType.EnableScript, ScriptActions.EnableScript },
            { ScriptActionType.DisableScript, ScriptActions.DisableScript },
            { ScriptActionType.CallSubroutine, ScriptActions.CallSubroutine },

            // Camera
            { ScriptActionType.MoveCameraTo, CameraActions.MoveCameraTo },
            { ScriptActionType.SetupCamera, CameraActions.SetupCamera },
            { ScriptActionType.MoveCameraAlongWaypointPath, CameraActions.MoveCameraAlongWaypointPath },
            { ScriptActionType.CameraModSetFinalZoom, CameraActions.CameraModSetFinalZoom },
            { ScriptActionType.CameraModSetFinalPitch, CameraActions.CameraModSetFinalPitch },
            { ScriptActionType.CameraModFinalLookToward, CameraActions.CameraModFinalLookToward },
            { ScriptActionType.CameraModLookToward, CameraActions.CameraModLookToward },

            // Camera - Terrain
            { ScriptActionType.TerrainRenderDisable, CameraActions.TerrainRenderDisable},

            // Team
            { ScriptActionType.TeamFollowWaypointsExact, TeamActions.TeamFollowWaypointsExact },

            // Unit
            { ScriptActionType.MoveNamedUnitTo, UnitActions.MoveNamedUnitTo },
            { ScriptActionType.NamedFollowWaypoints, UnitActions.NamedFollowWaypoints },
            { ScriptActionType.NamedStop, UnitActions.NamedStop }
        };

        public static ScriptingAction Get(ScriptAction action)
        {
            var actionType = action.ContentType;

            if (action.InternalName != null)
            {
                if (CachedEnumMap.TryGetValue(action.InternalName.Name, out var untypedActionType))
                {
                    actionType = (ScriptActionType) untypedActionType;
                }
            }

            if (!Actions.TryGetValue(actionType, out var actionFunction))
            {
                // TODO: Implement this action type.
                return MiscActions.NoOp;
            }

            return actionFunction;
        }

        private static readonly Dictionary<string, Enum> CachedEnumMap = new Dictionary<string, Enum>();

        static ActionLookup()
        {
            CachedEnumMap = IniParser.GetEnumMap<ScriptActionType>();
        }
    }
}

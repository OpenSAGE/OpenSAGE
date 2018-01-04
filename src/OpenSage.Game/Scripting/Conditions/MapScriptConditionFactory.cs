using System.Collections.Generic;
using OpenSage.Data.Map;
using OpenSage.Settings;

namespace OpenSage.Scripting.Conditions
{
    public static class MapScriptConditionFactory
    {
        private delegate MapScriptCondition CreateMapScriptDelegate(ScriptCondition action, SceneSettings sceneSettings);

        private static readonly Dictionary<ScriptConditionType, CreateMapScriptDelegate> Factories = new Dictionary<ScriptConditionType, CreateMapScriptDelegate>
        {
            { ScriptConditionType.False, (c, s) => new FalseCondition() },
            { ScriptConditionType.Flag, (c, s) => new FlagCondition(c) },
            { ScriptConditionType.True, (c, s) => new TrueCondition() },
            { ScriptConditionType.TimerExpired, (c, s) => new TimerExpiredCondition(c) },
            { ScriptConditionType.Counter, (c, s) => new CompareCounterCondition(c) }
        };

        public static MapScriptCondition Create(ScriptCondition condition, SceneSettings sceneSettings)
        {
            if (!Factories.TryGetValue(condition.ContentType, out var factory))
            {
                // TODO: Implement this condition type.
                return new FalseCondition();
            }

            return factory(condition, sceneSettings);
        }
    }
}

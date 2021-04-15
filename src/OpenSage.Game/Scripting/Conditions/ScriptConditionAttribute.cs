using System;

namespace OpenSage.Scripting
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ScriptConditionAttribute : Attribute
    {
        public ScriptConditionAttribute(ScriptConditionType conditionType, string uiName, string displayTemplate, params SageGame[] games)
        {
            ConditionType = conditionType;
            UIName = uiName;
            DisplayTemplate = displayTemplate;
            Games = games;
        }

        public readonly ScriptConditionType ConditionType;
        public readonly string UIName;
        public readonly string DisplayTemplate;
        public readonly SageGame[] Games;
    }
}

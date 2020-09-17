using System;

namespace OpenSage.Scripting
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ScriptActionAttribute : Attribute
    {
        public ScriptActionAttribute(ScriptActionType actionType, string uiName, string displayTemplate)
        {
            ActionType = actionType;
            UIName = uiName;
            DisplayTemplate = displayTemplate;
        }

        public readonly ScriptActionType ActionType;
        public readonly string UIName;
        public readonly string DisplayTemplate;
    }
}

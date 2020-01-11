using System.Collections.Generic;
using OpenSage.Data.Map;

namespace OpenSage.Scripting
{
    public sealed class MapScriptGroup
    {
        internal static MapScriptGroup[] Create(ScriptGroup[] scriptGroups)
        {
            var result = new MapScriptGroup[scriptGroups.Length];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = new MapScriptGroup(scriptGroups[i]);
            }

            return result;
        }

        private readonly MapScript[] _scripts;

        public IEnumerable<MapScript> Scripts => _scripts;

        public string Name { get; }

        public bool IsActive { get; set; }

        private readonly bool _isSubroutine;

        private MapScriptGroup(ScriptGroup scriptGroup)
        {
            Name = scriptGroup.Name;
            _scripts = MapScript.Create(scriptGroup.Scripts);
            IsActive = scriptGroup.IsActive;
            _isSubroutine = scriptGroup.IsSubroutine;
        }

        public void Execute(ScriptExecutionContext context)
        {
            if (_isSubroutine || !IsActive) return;

            foreach (var script in _scripts)
            {
                script.Execute(context);
            }
        }
    }
}

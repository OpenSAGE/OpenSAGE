using System.Collections.Generic;

namespace OpenSage.Scripting
{
    public sealed class MapScriptGroup
    {
        private readonly MapScript[] _scripts;

        public IEnumerable<MapScript> Scripts => _scripts;

        public string Name { get; }

        public bool IsActive { get; set; }

        private readonly bool _isSubroutine;

        public MapScriptGroup(
            string name,
            MapScript[] scripts,
            bool isInitiallyActive,
            bool isSubroutine)
        {
            Name = name;
            _scripts = scripts;
            IsActive = isInitiallyActive;
            _isSubroutine = isSubroutine;
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

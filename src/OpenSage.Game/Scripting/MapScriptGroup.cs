namespace OpenSage.Scripting
{
    public sealed class MapScriptGroup
    {
        private readonly MapScript[] _scripts;

        private enum ScriptGroupState
        {
            Inactive,
            SubroutineNotStarted,
            Active,
            SubroutineActive
        }

        private ScriptGroupState _state;

        public string Name { get; }

        public MapScriptGroup(
            string name,
            MapScript[] scripts,
            bool isInitiallyActive,
            bool isSubroutine)
        {
            Name = name;

            _scripts = scripts;

            if (isInitiallyActive)
            {
                _state = isSubroutine
                    ? ScriptGroupState.SubroutineNotStarted
                    : ScriptGroupState.Active;
            }
            else
            {
                _state = ScriptGroupState.Inactive;
            }
        }

        public void Execute(ScriptExecutionContext context)
        {
            // TODO: _isSubroutine

            switch (_state)
            {
                case ScriptGroupState.Inactive:
                case ScriptGroupState.SubroutineNotStarted:
                    return;
            }

            foreach (var script in _scripts)
            {
                script.Execute(context);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Scripting.Actions;
using OpenSage.Scripting.Conditions;

namespace OpenSage.Scripting
{
    public sealed class MapScript
    {
        private readonly MapScriptConditions _conditions;
        private readonly IReadOnlyList<MapScriptAction> _actionsIfTrue;
        private readonly IReadOnlyList<MapScriptAction> _actionsIfFalse;

        private readonly List<MapScriptAction> _currentActions;

        // Local to Execute(...), but here to avoid allocation every frame.
        private readonly List<MapScriptAction> _actionsToRemove;

        private enum ScriptState
        {
            Inactive,
            SubroutineNotStarted,
            NotStarted,
            Running,
            SubroutineRunning
        }

        private ScriptState _state;
        private bool? _currentConditionValue;

        private bool _deactivateUponSuccess;
        private uint _evaluationInterval;

        private TimeSpan _previousEvaluationTime;

        public string Name { get; }

        public MapScript(
            string name,
            MapScriptConditions conditions,
            IReadOnlyList<MapScriptAction> actionsIfTrue,
            IReadOnlyList<MapScriptAction> actionsIfFalse,
            bool isInitiallyActive,
            bool deactivateUponSuccess,
            bool isSubroutine,
            uint evaluationInterval)
        {
            Name = name;

            _conditions = conditions;
            _actionsIfTrue = actionsIfTrue;
            _actionsIfFalse = actionsIfFalse;

            _currentActions = new List<MapScriptAction>(Math.Max(actionsIfTrue.Count, actionsIfFalse.Count));
            _actionsToRemove = new List<MapScriptAction>(_currentActions.Count);

            _previousEvaluationTime = TimeSpan.Zero;

            if (isInitiallyActive)
            {
                _state = isSubroutine
                    ? ScriptState.SubroutineNotStarted
                    : ScriptState.NotStarted;
            }
            else
            {
                _state = ScriptState.Inactive;
            }

            _deactivateUponSuccess = deactivateUponSuccess;
            _evaluationInterval = evaluationInterval;
        }

        public void Execute(ScriptExecutionContext context)
        {
            // TODO: _isSubroutine

            switch (_state)
            {
                case ScriptState.Inactive:
                case ScriptState.SubroutineNotStarted:
                    return;
            }

            if (_evaluationInterval > 0)
            {
                var numSeconds = (context.UpdateTime.TotalGameTime - _previousEvaluationTime).TotalSeconds;
                if (numSeconds < _evaluationInterval)
                {
                    return;
                }
            }

            _previousEvaluationTime = context.UpdateTime.TotalGameTime;

            switch (_state)
            {
                case ScriptState.NotStarted:
                    _currentConditionValue = _conditions.Evaluate(context);
                    _currentActions.Clear();
                    var actions = _currentConditionValue.Value
                        ? _actionsIfTrue
                        : _actionsIfFalse;
                    foreach (var action in actions)
                    {
                        Console.WriteLine("Queueing: {0}", action);
                        _currentActions.Add(action);
                    }
                    _state = ScriptState.Running;
                    break;
            }

            foreach (var action in _currentActions)
            {
                var result = action.Execute(context);
                if (result == ScriptExecutionResult.Finished)
                {
                    Console.WriteLine("Finished {0}", action);
                    _actionsToRemove.Add(action);
                }
            }

            foreach (var actionToRemove in _actionsToRemove)
            {
                _currentActions.Remove(actionToRemove);
                actionToRemove.Reset();
            }

            _actionsToRemove.Clear();

            if (_currentActions.Count == 0)
            {
                _state = _deactivateUponSuccess && _currentConditionValue.Value
                    ? ScriptState.Inactive
                    : ScriptState.NotStarted;

                _currentActions.Clear();
                _currentConditionValue = null;
            }
        }

        internal void Restart()
        {
            foreach (var action in _actionsIfTrue.Concat(_actionsIfFalse))
            {
                action.Reset();
            }

            _state = ScriptState.NotStarted;
        }
    }
}

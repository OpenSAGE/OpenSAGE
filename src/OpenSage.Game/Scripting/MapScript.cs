using System.Collections.Generic;
using OpenSage.Data.Map;
using OpenSage.Scripting.Actions;
using OpenSage.Scripting.Conditions;

namespace OpenSage.Scripting
{
    public sealed class MapScript
    {
        internal static MapScript[] Create(Script[] scripts)
        {
            var result = new MapScript[scripts.Length];

            for (var i = 0; i < scripts.Length; i++)
            {
                result[i] = new MapScript(scripts[i]);
            }

            return result;
        }

        private readonly IReadOnlyList<ScriptOrCondition> _conditions;
        private readonly IReadOnlyList<ScriptAction> _actionsIfTrue;
        private readonly IReadOnlyList<ScriptAction> _actionsIfFalse;

        // Can this script run?
        public bool IsActive { get; set; }

        private bool? _currentConditionValue;

        private readonly bool _isSubroutine;
        private readonly bool _deactivateUponSuccess;

        private readonly uint _evaluationInterval;
        private uint? _framesSinceLastEvaluation;

        public string Name { get; }

        private MapScript(Script script)
        {
            Name = script.Name;

            _conditions = script.OrConditions;
            _actionsIfTrue = script.ActionsIfTrue;
            _actionsIfFalse = script.ActionsIfFalse;

            IsActive = script.IsActive;
            _isSubroutine = script.IsSubroutine;
            _deactivateUponSuccess = script.DeactivateUponSuccess;

            _evaluationInterval = script.EvaluationInterval * ScriptingSystem.TickRate;

            _framesSinceLastEvaluation = null;
        }

        public void Execute(ScriptExecutionContext context)
        {
            var shouldExecute = !_isSubroutine &&
                                IsActive &&
                                (_evaluationInterval == 0 ||
                                 _framesSinceLastEvaluation == null ||
                                 _framesSinceLastEvaluation >= _evaluationInterval);

            if (!shouldExecute)
            {
                // There's no need to increment _framesSinceLastEvaluation if _evaluationInterval is not used
                if (_evaluationInterval > 0 && _framesSinceLastEvaluation < _evaluationInterval)
                {
                    _framesSinceLastEvaluation++;
                }

                return;
            }

            _framesSinceLastEvaluation = 0;

            RunActions(context);
        }

        public void ExecuteAsSubroutine(ScriptExecutionContext context)
        {
            // Note: _evaluationInterval is checked here for compatiblity.
            // See Systems >> Scripting >> Subroutines in OpenSAGE docs for more information.
            var shouldExecute = _isSubroutine && IsActive && _evaluationInterval == 0;

            if (shouldExecute)
            {
                RunActions(context);
            }
        }

        private void RunActions(ScriptExecutionContext context)
        {
            _currentConditionValue = EvaluateConditions(context);

            var actions = _currentConditionValue.Value
                ? _actionsIfTrue
                : _actionsIfFalse;

            foreach (var action in actions)
            {
                var executor = ActionLookup.Get(action);
                var result = executor(action, context);
                if (result is ActionResult.ActionContinuation coroutine)
                {
                    context.Scripting.AddCoroutine(coroutine);
                }
            }

            var shouldDeactivate = _deactivateUponSuccess && actions.Count > 0;

            if (shouldDeactivate)
            {
                IsActive = false;
            }
        }

        private bool EvaluateConditions(ScriptExecutionContext context)
        {
            bool AllConditionsTrue(ScriptCondition[] conditions)
            {
                foreach (var condition in conditions)
                {
                    var result = ConditionLookup.Get(condition)(condition, context);
                    if (!result) return false;
                }

                return true;
            }

            foreach (var orCondition in _conditions)
            {
                var result = AllConditionsTrue(orCondition.Conditions);
                if (result) return true;
            }

            return false;
        }

    }
}

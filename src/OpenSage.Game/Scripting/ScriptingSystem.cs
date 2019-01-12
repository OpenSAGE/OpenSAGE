using System;
using System.Collections.Generic;
using OpenSage.Data.Map;

namespace OpenSage.Scripting
{
    public delegate bool ScriptingCondition(ScriptCondition condition, ScriptExecutionContext context);
    public delegate ActionResult ScriptingAction(ScriptAction action, ScriptExecutionContext context);

    // type ActionResult = ActionFinished | ActionContinuation
    public class ActionResult
    {
        public static readonly ActionResult Finished = new ActionFinished();

        // This is a zero-sized type which is used to signal that action execution has finished.
        public sealed class ActionFinished : ActionResult { }

        // This is effectively a stackless coroutine - update runs for one cycle, and then yields control back to the scripting system.
        public abstract class ActionContinuation : ActionResult
        {
            public abstract ActionResult Execute(ScriptExecutionContext context);
        }
    }

    public sealed class ScriptingSystem : GameSystem
    {
        // How many updates are performed per second?
        public const int TickRate = 30;

        private readonly ScriptExecutionContext _executionContext;

        private readonly List<ActionResult.ActionContinuation> _activeCoroutines;
        private readonly List<ActionResult.ActionContinuation> _finishedCoroutines;

        private MapScriptCollection _mapScripts;

        public Dictionary<string, bool> Flags { get; }
        public CounterCollection Counters { get; }
        public TimerCollection Timers { get; }

        public bool Active { get; set; }

        public event EventHandler<ScriptingSystem> OnUpdateFinished; 

        public ScriptingSystem(Game game) : base(game)
        {
            Flags = new Dictionary<string, bool>();
            Counters = new CounterCollection();
            Timers = new TimerCollection(Counters);

            _executionContext = new ScriptExecutionContext(game);

            _activeCoroutines = new List<ActionResult.ActionContinuation>();
            _finishedCoroutines = new List<ActionResult.ActionContinuation>();
        }

        internal override void OnSceneChanging()
        {
            _activeCoroutines.Clear();

            Flags.Clear();
            Counters.Clear();
            Timers.Clear();
        }

        internal override void OnSceneChanged()
        {
            _mapScripts = Game.Scene3D?.Scripts;
        }

        public void EnableScript(string name)
        {
            var mapScript = _mapScripts?.FindScript(name);
            if (mapScript != null)
            {
                mapScript.IsActive = true;
            }
        }

        public void DisableScript(string name)
        {
            var mapScript = _mapScripts?.FindScript(name);
            if (mapScript != null)
            {
                mapScript.IsActive = false;
            }
        }

        public void ExecuteSubroutine(string name)
        {
            var mapScript = _mapScripts?.FindScript(name);
            if (mapScript != null)
            {
                mapScript.ExecuteAsSubroutine(_executionContext);
            }
        }

        public void AddCoroutine(ActionResult.ActionContinuation coroutine)
        {
            _activeCoroutines.Add(coroutine);
        }

        public void ScriptingTick()
        {
            if (_mapScripts == null)
            {
                return;
            }

            if (!Active)
            {
                return;
            }

            foreach (var coroutine in _activeCoroutines)
            {
                var result = coroutine.Execute(_executionContext);
                if (result is ActionResult.ActionFinished)
                {
                    _finishedCoroutines.Add(coroutine);
                }
            }

            foreach (var coroutine in _finishedCoroutines)
            {
                _activeCoroutines.Remove(coroutine);
            }

            _mapScripts.Execute(_executionContext);

            OnUpdateFinished?.Invoke(this, this);

            Timers.Update();
        }
    }
}

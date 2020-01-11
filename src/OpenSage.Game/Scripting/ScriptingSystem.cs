using System;
using System.Collections.Generic;
using System.Linq;
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
        public static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();

        // How many updates are performed per second?
        public const int TickRate = 30;

        private readonly ScriptExecutionContext _executionContext;

        private readonly List<ActionResult.ActionContinuation> _activeCoroutines;
        private readonly List<ActionResult.ActionContinuation> _finishedCoroutines;

        private MapScriptCollection[] _playerScripts;

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
            _playerScripts = Game.Scene3D?.PlayerScripts;
        }

        public MapScript FindScript(string name)
        {
            if (_playerScripts == null)
            {
                return null;
            }

            for (int i = 0; i < _playerScripts.Length; i++)
            {
                var script = _playerScripts[i].FindScript(name);
                if (script != null)
                {
                   return null;
                }
            }

            return null;
        }

        public void AddCoroutine(ActionResult.ActionContinuation coroutine)
        {
            _activeCoroutines.Add(coroutine);
        }

        public void ScriptingTick()
        {
            if (_playerScripts == null)
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

            foreach (var playerScripts in _playerScripts)
            {
                playerScripts.Execute(_executionContext);
            }

            OnUpdateFinished?.Invoke(this, this);

            Timers.Update();
        }
    }
}

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

        private readonly List<ScriptComponent> _scripts;
        private readonly Dictionary<string, MapScript> _scriptsByName;

        private readonly ScriptExecutionContext _executionContext;

        private readonly List<ActionResult.ActionContinuation> _activeCoroutines;
        private readonly List<ActionResult.ActionContinuation> _finishedCoroutines;

        public Dictionary<string, bool> Flags { get; }
        public CounterCollection Counters { get; }
        public TimerCollection Timers { get; }

        public bool Active { get; set; }

        public ulong Frame { get; private set; }

        private bool _30hzHack = true;

        public event EventHandler<ScriptingSystem> OnUpdateFinished; 

        public ScriptingSystem(Game game) : base(game)
        {
            RegisterComponentList(_scripts = new List<ScriptComponent>());

            Flags = new Dictionary<string, bool>();
            Counters = new CounterCollection();
            Timers = new TimerCollection(Counters);

            _executionContext = new ScriptExecutionContext(game);

            _scriptsByName = new Dictionary<string, MapScript>();

            _activeCoroutines = new List<ActionResult.ActionContinuation>();
            _finishedCoroutines = new List<ActionResult.ActionContinuation>();
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            if (component is ScriptComponent)
            {
                UpdateScriptIndex();
            }
            base.OnEntityComponentAdded(component);
        }

        internal override void OnEntityComponentRemoved(EntityComponent component)
        {
            if (component is ScriptComponent)
            {
                UpdateScriptIndex();
            }
            base.OnEntityComponentRemoved(component);
        }

        // Note: This is only updated when ScriptComponents are added or removed.
        // Since ScriptComponent is not immutable, it can be modified and thus contain
        // scripts that are not included in the index. 
        private void UpdateScriptIndex()
        {
            _scriptsByName.Clear();

            foreach (var component in _scripts)
            {
                foreach (var script in component.Scripts)
                {
                    _scriptsByName[script.Name] = script;
                }

                foreach (var scriptGroup in component.ScriptGroups)
                {
                    foreach (var script in scriptGroup.Scripts)
                    {
                        _scriptsByName[script.Name] = script;
                    }
                }
            }
        }

        internal override void OnSceneChanging()
        {
            _scriptsByName.Clear();
            _activeCoroutines.Clear();

            Flags.Clear();
            Counters.Clear();
            Timers.Clear();

            Frame = 0;

            base.OnSceneChanging();
        }

        internal override void OnSceneChanged()
        {
            UpdateScriptIndex();
        }

        public void EnableScript(string name)
        {
            if (_scriptsByName.TryGetValue(name, out var mapScript))
            {
                mapScript.IsActive = true;
            }
        }

        public void DisableScript(string name)
        {
            if (_scriptsByName.TryGetValue(name, out var mapScript))
            {
                mapScript.IsActive = false;
            }
        }

        public void ExecuteSubroutine(string name)
        {
            if (_scriptsByName.TryGetValue(name, out var mapScript))
            {
                mapScript.ExecuteAsSubroutine(_executionContext);
            }
        }

        public void AddCoroutine(ActionResult.ActionContinuation coroutine)
        {
            _activeCoroutines.Add(coroutine);
        }

        public override void Update(GameTime gameTime)
        {
            // TODO: Remove this hack when we have separate update and render loops.
            _30hzHack = !_30hzHack;
            if (_30hzHack) return;

            if (!Active) return;

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

            foreach (var scriptComponent in _scripts)
            {
                scriptComponent.Execute(_executionContext);
            }

            OnUpdateFinished?.Invoke(this, this);

            Timers.Update();

            Frame++;
        }
    }
}

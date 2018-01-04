using System;
using System.Collections.Generic;

namespace OpenSage.Scripting
{
    public sealed class ScriptingSystem : GameSystem
    {
        private readonly List<ScriptComponent> _scripts;
        private readonly Dictionary<string, MapScript> _scriptsByName;

        private readonly ScriptExecutionContext _executionContext;

        public Dictionary<string, int> Counters { get; }
        public Dictionary<string, bool> Flags { get; }
        public Dictionary<string, ScriptTimer> Timers { get; }

        public bool Active { get; set; }

        public ScriptingSystem(Game game) 
            : base(game)
        {
            RegisterComponentList(_scripts = new List<ScriptComponent>());

            Counters = new Dictionary<string, int>();
            Flags = new Dictionary<string, bool>();
            Timers = new Dictionary<string, ScriptTimer>();

            _executionContext = new ScriptExecutionContext(game);
            _scriptsByName = new Dictionary<string, MapScript>();
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

        internal void RestartScript(string name)
        {
            _scriptsByName.TryGetValue(name, out var script);
            script?.Restart();
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                // TODO: Timers should always update their counters 30 times a second.
                // TODO: Make sure timer updates are never missed, as some scripts may rely on specific values.
                foreach (var kv in Timers)
                {
                    var name = kv.Key;
                    var timer = kv.Value;

                    if (gameTime.TotalGameTime >= timer.ExpirationTime)
                    {
                        Counters[name] = 0;
                        timer.Expired = true;
                        // TODO: Should we remove expired timers on the next frame?
                    }
                    else
                    {
                        // TODO: Should this be rounded up, rounded down, or truncated?
                        Counters[name] = (int) (timer.ExpirationTime.TotalSeconds - gameTime.TotalGameTime.TotalSeconds);
                    }
                }

                foreach (var scriptComponent in _scripts)
                {
                    scriptComponent.Execute(_executionContext);
                }
            }
        }

        internal override void OnSceneChange()
        {
            _scriptsByName.Clear();
            base.OnSceneChange();
        }
    }

    public sealed class ScriptTimer
    {
        public TimeSpan ExpirationTime { get; }
        public bool Expired { get; internal set; }

        public ScriptTimer(TimeSpan expirationTime)
        {
            ExpirationTime = expirationTime;
        }
    }
}

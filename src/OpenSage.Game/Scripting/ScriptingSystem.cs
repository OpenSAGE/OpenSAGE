using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenSage.Scripting
{
    public sealed class ScriptingSystem : GameSystem
    {
        private readonly List<ScriptComponent> _scripts;

        private readonly CoroutineScheduler _coroutineScheduler;
        private readonly Coroutine _coroutine;

        private readonly Dictionary<ScriptComponent, Task> _coroutineInstances;

        public Dictionary<string, bool> Flags { get; }
        public Dictionary<string, ScriptTimer> Timers { get; }

        public ScriptingSystem(Game game) 
            : base(game)
        {
            RegisterComponentList(_scripts = new List<ScriptComponent>());

            _coroutineScheduler = new CoroutineScheduler();
            _coroutine = new Coroutine(_coroutineScheduler, game);

            _coroutineInstances = new Dictionary<ScriptComponent, Task>();

            Flags = new Dictionary<string, bool>();
            Timers = new Dictionary<string, ScriptTimer>();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var timer in Timers.Values)
            {
                if (gameTime.TotalGameTime >= timer.ExpirationTime)
                {
                    timer.Expired = true;
                    // TODO: Should we remove expired timers on the next frame?
                }
            }

            _coroutineScheduler.Update();

            foreach (var scriptComponent in _scripts)
            {
                if (!_coroutineInstances.TryGetValue(scriptComponent, out var coroutineInstance)
                    || coroutineInstance.IsCompleted)
                {
                    _coroutineInstances[scriptComponent] = _coroutine.Run(scriptComponent.Execute);
                }
            }
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

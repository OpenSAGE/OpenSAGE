using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenSage.Scripting
{
    public sealed class ScriptingSystem : GameSystem
    {
        private readonly List<ScriptComponent> _scripts;

        private readonly ScriptExecutionContext _executionContext;

        public Dictionary<string, int> Counters { get; }
        public Dictionary<string, bool> Flags { get; }
        public Dictionary<string, ScriptTimer> Timers { get; }

        public ScriptingSystem(Game game) 
            : base(game)
        {
            RegisterComponentList(_scripts = new List<ScriptComponent>());

            Counters = new Dictionary<string, int>();
            Flags = new Dictionary<string, bool>();
            Timers = new Dictionary<string, ScriptTimer>();

            _executionContext = new ScriptExecutionContext(game);
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

            foreach (var scriptComponent in _scripts)
            {
                scriptComponent.Execute(_executionContext);
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

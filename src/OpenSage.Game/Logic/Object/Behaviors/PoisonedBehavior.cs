using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class PoisonedBehaviorModule : BehaviorModule
    {
        GameObject _gameObject;
        PoisonedBehaviorModuleData _moduleData;
        private TimeSpan _lastUpdate;

        internal PoisonedBehaviorModule(GameObject gameObject, GameContext context, PoisonedBehaviorModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var currentTime = context.Time.TotalTime;

            if (_lastUpdate == null)
            {
                _lastUpdate = currentTime;
                return;
            }

            if ((currentTime - _lastUpdate).TotalMilliseconds > _moduleData.PoisonDamageInterval)
            {
                // what is the poison damage?
                // is there a poison nugget / weapon?
                _lastUpdate = currentTime;
            }
        }
    }

    public sealed class PoisonedBehaviorModuleData : UpdateModuleData
    {
        internal static PoisonedBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PoisonedBehaviorModuleData> FieldParseTable = new IniParseTable<PoisonedBehaviorModuleData>
        {
            { "PoisonDamageInterval", (parser, x) => x.PoisonDamageInterval = parser.ParseInteger() },
            { "PoisonDuration", (parser, x) => x.PoisonDuration = parser.ParseInteger() }
        };

        /// <summary>
        /// Frequency (in milliseconds) to apply poison damage.
        /// </summary>
        public int PoisonDamageInterval { get; private set; }

        /// <summary>
        /// Amount of time to continue being damaged after last hit by poison damage.
        /// </summary>
        public int PoisonDuration { get; private set; }
    }
}

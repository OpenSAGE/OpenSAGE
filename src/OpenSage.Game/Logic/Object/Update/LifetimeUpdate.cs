using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class LifetimeUpdate : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly LifetimeUpdateModuleData _moduleData;

        private TimeSpan _lifeTime;
        private bool _initial = true;

        public LifetimeUpdate(GameObject gameObject, LifetimeUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_initial)
            {
                _lifeTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(context.GameContext.Random.Next((int) _moduleData.MinLifetime, (int) _moduleData.MaxLifetime));
                _initial = false;
            }

            if (context.Time.TotalTime > _lifeTime) _gameObject.Die(_moduleData.DeathType, context.Time);
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknown = reader.ReadUInt32();
        }
    }

    public sealed class LifetimeUpdateModuleData : UpdateModuleData
    {
        internal static LifetimeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LifetimeUpdateModuleData> FieldParseTable = new IniParseTable<LifetimeUpdateModuleData>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseLong() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseLong() },
            { "WaitForWakeUp", (parser, x) => x.WaitForWakeUp = parser.ParseBoolean() },
            { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() }
        };

        public long MinLifetime { get; private set; }
        public long MaxLifetime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool WaitForWakeUp { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public DeathType DeathType { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new LifetimeUpdate(gameObject, this);
        }
    }
}

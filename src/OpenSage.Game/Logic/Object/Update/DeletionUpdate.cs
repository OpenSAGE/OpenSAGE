using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal class DeletionUpdate : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly DeletionUpdateModuleData _moduleData;

        private TimeSpan _lifeTime;
        private bool _initial = true;

        private uint _unknown;

        public DeletionUpdate(GameObject gameObject, DeletionUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_initial)
            {
                _lifeTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(context.GameContext.Random.Next((int)_moduleData.MinLifetime, (int)_moduleData.MaxLifetime));
                _initial = false;
            }

            if (context.Time.TotalTime > _lifeTime)
            {
                _gameObject.Destroy();
            }
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadUInt32(ref _unknown);
        }
    }


    public sealed class DeletionUpdateModuleData : UpdateModuleData
    {
        internal static DeletionUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DeletionUpdateModuleData> FieldParseTable = new IniParseTable<DeletionUpdateModuleData>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseLong() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseLong() }
        };

        public long MinLifetime { get; private set; }
        public long MaxLifetime { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DeletionUpdate(gameObject, this);
        }
    }
}

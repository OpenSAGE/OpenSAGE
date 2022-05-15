using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class LifetimeUpdate : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly LifetimeUpdateModuleData _moduleData;

        private LogicFrame _frameToDie;

        // TODO: Should this be public?
        public LogicFrame FrameToDie
        {
            get => _frameToDie;
            set => _frameToDie = value;
        }

        public LifetimeUpdate(GameObject gameObject, LifetimeUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;

            var lifetimeFrames = gameObject.GameContext.Random.Next(
                (int)moduleData.MinLifetime.Value,
                (int)moduleData.MaxLifetime.Value);

            _frameToDie = gameObject.GameContext.GameLogic.CurrentFrame + new LogicFrameSpan((uint)lifetimeFrames);
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (context.LogicFrame >= _frameToDie)
            {
                _gameObject.Die(_moduleData.DeathType);
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistLogicFrame(ref _frameToDie);
        }
    }

    public sealed class LifetimeUpdateModuleData : UpdateModuleData
    {
        internal static LifetimeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LifetimeUpdateModuleData> FieldParseTable = new IniParseTable<LifetimeUpdateModuleData>
        {
            { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "WaitForWakeUp", (parser, x) => x.WaitForWakeUp = parser.ParseBoolean() },
            { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() }
        };

        public LogicFrameSpan MinLifetime { get; private set; }
        public LogicFrameSpan MaxLifetime { get; private set; }

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

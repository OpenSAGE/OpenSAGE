using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class InstantDeathBehavior : DieModule
    {
        private readonly GameObject _gameObject;
        private readonly InstantDeathBehaviorModuleData _moduleData;

        internal InstantDeathBehavior(GameObject gameObject, InstantDeathBehaviorModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType)
        {
            if (!_moduleData.DeathTypes.Get(deathType))
            {
                return;
            }

            context.GameObject.Destroy();

            Matrix4x4.Decompose(context.GameObject.TransformMatrix, out _, out var rotation, out var translation);

            _moduleData.FX?.Value?.Execute(new FXListExecutionContext(
                rotation,
                translation,
                context.GameContext));
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public sealed class InstantDeathBehaviorModuleData : BehaviorModuleData
    {
        internal static InstantDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InstantDeathBehaviorModuleData> FieldParseTable = new IniParseTable<InstantDeathBehaviorModuleData>
        {
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() },
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "FX", (parser, x) => x.FX = parser.ParseFXListReference() },
            { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
        };

        public ObjectStatus RequiredStatus { get; private set; }
        public BitArray<DeathType> DeathTypes { get; private set; }
        public LazyAssetReference<FXList> FX { get; private set; }
        public string OCL { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new InstantDeathBehavior(gameObject, this);
        }
    }
}

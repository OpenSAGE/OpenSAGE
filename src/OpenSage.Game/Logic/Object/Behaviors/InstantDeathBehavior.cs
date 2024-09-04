using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;

namespace OpenSage.Logic.Object
{
    public sealed class InstantDeathBehavior : DieModule
    {
        private readonly GameObject _gameObject;
        private readonly InstantDeathBehaviorModuleData _moduleData;

        internal InstantDeathBehavior(GameObject gameObject, InstantDeathBehaviorModuleData moduleData)
            : base(moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
        {
            _gameObject.GameContext.GameLogic.DestroyObject(_gameObject);

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

    public sealed class InstantDeathBehaviorModuleData : DieModuleData
    {
        internal static InstantDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<InstantDeathBehaviorModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<InstantDeathBehaviorModuleData>
            {
                { "FX", (parser, x) => x.FX = parser.ParseFXListReference() },
                { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
            });

        public LazyAssetReference<FXList> FX { get; private set; }
        public string OCL { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new InstantDeathBehavior(gameObject, this);
        }
    }
}

using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;

namespace OpenSage.Logic.Object;

public sealed class InstantDeathBehavior : DieModule
{
    private readonly InstantDeathBehaviorModuleData _moduleData;

    internal InstantDeathBehavior(GameObject gameObject, GameEngine gameEngine, InstantDeathBehaviorModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
    {
        GameEngine.GameLogic.DestroyObject(GameObject);

        Matrix4x4.Decompose(context.GameObject.TransformMatrix, out _, out var rotation, out var translation);

        _moduleData.FX?.Value?.Execute(new FXListExecutionContext(
            rotation,
            translation,
            context.GameEngine));
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

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new InstantDeathBehavior(gameObject, gameEngine, this);
    }
}

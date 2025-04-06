using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class DefaultProductionExitUpdate : UpdateModule, IHasRallyPoint, IProductionExit
{
    public RallyPointManager RallyPointManager { get; } = new();

    private readonly DefaultProductionExitUpdateModuleData _moduleData;

    internal DefaultProductionExitUpdate(GameObject gameObject, IGameEngine gameEngine, DefaultProductionExitUpdateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;
    }

    Vector3 IProductionExit.GetUnitCreatePoint() => _moduleData.UnitCreatePoint;

    Vector3? IProductionExit.GetNaturalRallyPoint() => _moduleData.NaturalRallyPoint;

    public override UpdateSleepTime Update()
    {
        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistObject(RallyPointManager);
    }
}

public sealed class DefaultProductionExitUpdateModuleData : UpdateModuleData
{
    internal static DefaultProductionExitUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<DefaultProductionExitUpdateModuleData> FieldParseTable = new IniParseTable<DefaultProductionExitUpdateModuleData>
    {
        { "UnitCreatePoint", (parser, x) => x.UnitCreatePoint = parser.ParseVector3() },
        { "NaturalRallyPoint", (parser, x) => x.NaturalRallyPoint = parser.ParseVector3() },
        { "UseSpawnRallyPoint", (parser, x) => x.UseSpawnRallyPoint = parser.ParseBoolean() },
    };

    public Vector3 UnitCreatePoint { get; private set; }

    /// <summary>
    /// <see cref="NaturalRallyPoint.X"/> must match <see cref="ObjectDefinition.GeometryMajorRadius"/>.
    /// </summary>
    public Vector3 NaturalRallyPoint { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool UseSpawnRallyPoint { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new DefaultProductionExitUpdate(gameObject, gameEngine, this);
    }
}

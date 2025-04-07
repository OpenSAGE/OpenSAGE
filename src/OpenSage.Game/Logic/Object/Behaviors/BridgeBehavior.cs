using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object;

public sealed class BridgeBehavior : UpdateModule
{
    private readonly ObjectId[] _towerIds = new ObjectId[4];

    public BridgeBehavior(GameObject gameObject, IGameEngine gameEngine)
        : base(gameObject, gameEngine)
    {
    }

    public ObjectId GetTowerId(BridgeTowerType towerType)
    {
        return _towerIds[(int)towerType];
    }

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

        reader.PersistArray(
            _towerIds,
            static (StatePersister persister, ref ObjectId item) =>
            {
                persister.PersistObjectIdValue(ref item);
            });

        reader.SkipUnknownBytes(7);
    }
}

/// <summary>
/// Special-case logic allows for ParentObject to be specified as a bone name to allow other
/// objects to appear on the bridge.
/// </summary>
public sealed class BridgeBehaviorModuleData : BehaviorModuleData
{
    internal static BridgeBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<BridgeBehaviorModuleData> FieldParseTable = new IniParseTable<BridgeBehaviorModuleData>
    {
        { "LateralScaffoldSpeed", (parser, x) => x.LateralScaffoldSpeed = parser.ParseFloat() },
        { "VerticalScaffoldSpeed", (parser, x) => x.VerticalScaffoldSpeed = parser.ParseFloat() },

        { "BridgeDieOCL", (parser, x) => x.BridgeDieOCLs.Add(BridgeDieObjectCreationList.Parse(parser)) },
        { "BridgeDieFX", (parser, x) => x.BridgeDieFXs.Add(BridgeDieFX.Parse(parser)) }
    };

    public float LateralScaffoldSpeed { get; private set; }
    public float VerticalScaffoldSpeed { get; private set; }

    public List<BridgeDieObjectCreationList> BridgeDieOCLs { get; } = new List<BridgeDieObjectCreationList>();
    public List<BridgeDieFX> BridgeDieFXs { get; } = new List<BridgeDieFX>();

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new BridgeBehavior(gameObject, gameEngine);
    }
}

public sealed class BridgeDieObjectCreationList
{
    internal static BridgeDieObjectCreationList Parse(IniParser parser)
    {
        return new BridgeDieObjectCreationList
        {
            OCL = parser.ParseAttribute("OCL", parser.ScanAssetReference),
            Delay = parser.ParseAttributeInteger("Delay"),
            Bone = parser.ParseAttribute("Bone", parser.ScanBoneName)
        };
    }

    public string OCL { get; private set; }
    public int Delay { get; private set; }
    public string Bone { get; private set; }
}

public sealed class BridgeDieFX
{
    internal static BridgeDieFX Parse(IniParser parser)
    {
        return new BridgeDieFX
        {
            FX = parser.ParseAttribute("FX", parser.ScanAssetReference),
            Delay = parser.ParseAttributeInteger("Delay"),
            Bone = parser.ParseAttribute("Bone", parser.ScanBoneName)
        };
    }

    public string FX { get; private set; }
    public int Delay { get; private set; }
    public string Bone { get; private set; }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class AutoFindHealingUpdate : UpdateModule
{
    private readonly AutoFindHealingUpdateModuleData _moduleData;

    public AutoFindHealingUpdate(GameObject gameObject, IGameEngine gameEngine, AutoFindHealingUpdateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;
    }

    private LogicFrameSpan _framesUntilNextScan;

    public override UpdateSleepTime Update()
    {
        if (_framesUntilNextScan == LogicFrameSpan.Zero)
        {
            _framesUntilNextScan = _moduleData.ScanRate;

            // TODO: Find healing.
        }
        else
        {
            _framesUntilNextScan--;
        }

        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistLogicFrameSpan(ref _framesUntilNextScan);
    }
}

/// <summary>
/// Searches for a nearby healing station. AI only.
/// </summary>
public sealed class AutoFindHealingUpdateModuleData : UpdateModuleData
{
    internal static AutoFindHealingUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<AutoFindHealingUpdateModuleData> FieldParseTable = new IniParseTable<AutoFindHealingUpdateModuleData>
    {
        { "ScanRate", (parser, x) => x.ScanRate = parser.ParseTimeMillisecondsToLogicFrames() },
        { "ScanRange", (parser, x) => x.ScanRange = parser.ParseInteger() },
        { "NeverHeal", (parser, x) => x.NeverHeal = parser.ParseFloat() },
        { "AlwaysHeal", (parser, x) => x.AlwaysHeal = parser.ParseFloat() }
    };

    public LogicFrameSpan ScanRate { get; private set; }
    public int ScanRange { get; private set; }
    public float NeverHeal { get; private set; }
    public float AlwaysHeal { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new AutoFindHealingUpdate(gameObject, gameEngine, this);
    }
}

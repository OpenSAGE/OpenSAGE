using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Utilities;

namespace OpenSage.Logic.Object;

internal class DeletionUpdate : UpdateModule
{
    private readonly DeletionUpdateModuleData _moduleData;

    private LogicFrame _dieFrame;

    public DeletionUpdate(GameObject gameObject, IGameEngine gameEngine, DeletionUpdateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;

        SetWakeFrame(UpdateSleepTime.Frames(CalculateSleepDelay(_moduleData.MinLifetime, _moduleData.MaxLifetime)));
    }

    private LogicFrameSpan CalculateSleepDelay(LogicFrameSpan minFrames, LogicFrameSpan maxFrames)
    {
        var delay = GameEngine.GameLogic.Random.NextLogicFrameSpan(minFrames, maxFrames);
        if (delay < new LogicFrameSpan(1))
        {
            // If the delay is less than 1 frame, set it to 1 frame.
            return new LogicFrameSpan(1);
        }
        _dieFrame = GameEngine.GameLogic.CurrentFrame + delay;
        return delay;
    }

    public override UpdateSleepTime Update()
    {
        GameEngine.GameLogic.DestroyObject(GameObject);
        return UpdateSleepTime.Forever;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        base.Load(reader);

        reader.PersistLogicFrame(ref _dieFrame);
    }

    internal override void DrawInspector()
    {
        base.DrawInspector();
        ImGui.LabelText("Frame to delete", _dieFrame.ToString());
    }
}


public sealed class DeletionUpdateModuleData : UpdateModuleData
{
    internal static DeletionUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<DeletionUpdateModuleData> FieldParseTable = new IniParseTable<DeletionUpdateModuleData>
    {
        { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseTimeMillisecondsToLogicFrames() },
        { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseTimeMillisecondsToLogicFrames() }
    };

    public LogicFrameSpan MinLifetime { get; private set; }
    public LogicFrameSpan MaxLifetime { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new DeletionUpdate(gameObject, gameEngine, this);
    }
}

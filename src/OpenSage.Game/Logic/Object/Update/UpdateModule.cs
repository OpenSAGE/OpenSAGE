using ImGuiNET;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public abstract class UpdateModule : BehaviorModule, IUpdateModule
{
    private UpdateFrame _nextUpdateFrame;

    protected virtual UpdateOrder UpdateOrder => UpdateOrder.Order2;

    UpdateOrder IUpdateModule.UpdatePhase => UpdateOrder;

    // These properties are used for sleepy update stuff in GameLogic.

    internal uint Priority => _nextUpdateFrame.RawValue;
    internal LogicFrame NextCallFrame
    {
        get => _nextUpdateFrame.Frame;
        set
        {
            // Anything greater than "forever" is still "forever".
            // This makes SetWakeFrame comparisons simpler and more efficient.
            if (value.Value > UpdateSleepTime.SleepForever)
            {
                value = new LogicFrame(UpdateSleepTime.SleepForever);
            }
            _nextUpdateFrame = new UpdateFrame(value, UpdateOrder);
        }
    }

    internal UpdateOrder NextCallPhase => _nextUpdateFrame.UpdateOrder;
    internal int IndexInLogic = -1;
    internal GameObject ParentGameObject => GameObject;

    public virtual BitArray<DisabledType> DisabledTypesToProcess { get; } = new BitArray<DisabledType>();

    protected UpdateModule(GameObject gameObject, IGameEngine gameEngine)
        : base(gameObject, gameEngine)
    {
        _nextUpdateFrame.UpdateOrder = UpdateOrder;
    }

    void IUpdateModule.Update()
    {
        if (GameEngine.GameLogic.CurrentFrame < _nextUpdateFrame.Frame)
        {
            return;
        }

        var sleepTime = Update();

        _nextUpdateFrame = new UpdateFrame(GameEngine.GameLogic.CurrentFrame + sleepTime.FrameSpan, UpdateOrder);
    }

    public abstract UpdateSleepTime Update();

    // Yes, protected. Modules should only wake themselves up.
    protected void SetWakeFrame(UpdateSleepTime wakeDelay)
    {
        var now = GameEngine.GameLogic.CurrentFrame;
        GameEngine.GameLogic.AwakenUpdateModule(GameObject, this, now + wakeDelay.FrameSpan);
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        var currentUpdateOrder = _nextUpdateFrame.UpdateOrder;
        reader.PersistUpdateFrame(ref _nextUpdateFrame);
        if (reader.Mode == StatePersistMode.Read && _nextUpdateFrame.UpdateOrder != currentUpdateOrder)
        {
            throw new InvalidStateException();
        }
    }

    internal override void DrawInspector()
    {
        ImGui.LabelText("Next update frame", _nextUpdateFrame.Frame.ToString());
    }
}

public readonly struct UpdateSleepTime
{
    public static readonly UpdateSleepTime None = new(new LogicFrameSpan(1));

    internal const uint SleepForever = 0x3fffffff;

    // we use 0x3fffffff so that we can add offsets and not overflow...
    // and also 'cuz we shift the value up by two bits for the phase.
    // note that at 30fps, this is ~414 days...
    public static readonly UpdateSleepTime Forever = new(new LogicFrameSpan(SleepForever));

    public static UpdateSleepTime Frames(LogicFrameSpan frames) => new UpdateSleepTime(frames);

    public readonly LogicFrameSpan FrameSpan;

    private UpdateSleepTime(LogicFrameSpan frames)
    {
        FrameSpan = frames;
    }
}

public struct UpdateFrame
{
    public uint RawValue;

    public UpdateFrame(LogicFrame frame, UpdateOrder updateOrder)
    {
        Frame = frame;
        UpdateOrder = updateOrder;
    }

    public LogicFrame Frame
    {
        get => new LogicFrame(RawValue >> 2);
        set => RawValue = (value.Value << 2) | (RawValue & 0x3);
    }

    public UpdateOrder UpdateOrder
    {
        get => (UpdateOrder)(RawValue & 0x3);
        set => RawValue = (RawValue & 0xFFFFFFFC) | (byte)(value);
    }
}

public enum UpdateOrder : byte
{
    Order0 = 0,
    Order1 = 1,
    Order2 = 2,
    Order3 = 3,
}

internal interface IUpdateModule
{
    UpdateOrder UpdatePhase { get; }

    void Update();
}

public interface IProjectileUpdate
{
    bool ProjectileHandleCollision(GameObject other);

    void ProjectileNowJammed();
}

public abstract class UpdateModuleData : ContainModuleData
{
    public override ModuleKinds ModuleKinds => ModuleKinds.Update;
}

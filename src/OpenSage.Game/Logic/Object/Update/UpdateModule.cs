using ImGuiNET;

namespace OpenSage.Logic.Object;

public abstract class UpdateModule : BehaviorModule, IUpdateModule
{
    private UpdateFrame _nextUpdateFrame;

    protected internal UpdateFrame NextUpdateFrame => _nextUpdateFrame;

    protected virtual LogicFrameSpan FramesBetweenUpdates { get; } = new(1);

    protected virtual UpdateOrder UpdateOrder => UpdateOrder.Order2;

    UpdateOrder IUpdateModule.UpdatePhase => UpdateOrder;

    protected UpdateModule(GameObject gameObject, GameContext context)
        : base(gameObject, context)
    {
        _nextUpdateFrame.UpdateOrder = UpdateOrder;
    }

    private protected virtual void RunUpdate(BehaviorUpdateContext context) { }

    void IUpdateModule.Update(BehaviorUpdateContext context)
    {
        Update(context);
    }

    // todo: seal this method?
    internal virtual void Update(BehaviorUpdateContext context)
    {
        if (context.LogicFrame.Value < _nextUpdateFrame.Frame)
        {
            return;
        }

        SetNextUpdateFrame(context.LogicFrame + FramesBetweenUpdates);
        RunUpdate(context);

        var sleepTime = Update();

        _nextUpdateFrame = new UpdateFrame(context.LogicFrame + sleepTime.FrameSpan, UpdateOrder);
    }

    // TODO: Remove other Update methods after everything uses this.
    public virtual UpdateSleepTime Update()
    {
        return UpdateSleepTime.Frames(FramesBetweenUpdates);
    }

    protected void SetNextUpdateFrame(LogicFrame frame)
    {
        // If we are already awake, don't reset our wake frame. See GameLogic::friend_awakenUpdateModule.
        if (GameObject != null)
        {
            var now = Context.GameLogic.CurrentFrame.Value;
            if (_nextUpdateFrame.Frame == now && frame.Value == now + 1)
            {
                return;
            }
        }

        _nextUpdateFrame = new UpdateFrame(frame, UpdateOrder);
    }

    // Yes, protected. Modules should only wake themselves up.
    protected void SetWakeFrame(UpdateSleepTime wakeDelay)
    {
        SetNextUpdateFrame(Context.GameLogic.CurrentFrame + wakeDelay.FrameSpan);
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

    // we use 0x3fffffff so that we can add offsets and not overflow...
    // and also 'cuz we shift the value up by two bits for the phase.
    // note that at 30fps, this is ~414 days...
    public static readonly UpdateSleepTime Forever = new(new LogicFrameSpan(0x3fffffff));

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
        Frame = frame.Value;
        UpdateOrder = updateOrder;
    }

    public uint Frame
    {
        get => RawValue >> 2;
        set => RawValue = (value << 2) | (RawValue & 0x3);
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

    void Update(BehaviorUpdateContext context);
}

public interface IProjectileUpdate
{
    bool ProjectileHandleCollision(GameObject other);
}

public abstract class UpdateModuleData : ContainModuleData
{
    public override ModuleKinds ModuleKinds => ModuleKinds.Update;
}

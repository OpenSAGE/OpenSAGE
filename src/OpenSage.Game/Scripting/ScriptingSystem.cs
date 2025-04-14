using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Logic;
using OpenSage.Logic.Object;

namespace OpenSage.Scripting;

public delegate bool ScriptingCondition(ScriptExecutionContext context, ScriptCondition condition);
public delegate void ScriptingAction(ScriptExecutionContext context, ScriptAction action);

public sealed class ScriptingSystem : GameSystem, IPersistableObject
{
    private readonly List<SequentialScript> _sequentialScripts = new();

    public static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();

    // How many updates are performed per second?
    public readonly uint TickRate;

    private readonly ScriptExecutionContext _executionContext;

    internal readonly CameraFadeOverlay CameraFadeOverlay;

    private readonly ScriptingFlag[] _flags = new ScriptingFlag[128];
    private ushort _numFlags;

    private readonly ScriptingCounter[] _counters = new ScriptingCounter[128];
    private ushort _numCounters;

    private readonly List<AttackPriority> _attackPriorities = new();
    private readonly List<ObjectNameAndId> _unknownSomethings = new();
    private readonly List<ObjectNameAndId>[] _specialPowers;
    private readonly List<ObjectNameAndId>[] _unknownSuperweaponArray;
    private readonly List<ObjectNameAndId>[] _upgrades;
    private readonly ScienceSet[] _sciences;
    private BreezeInfo _breezeInfo;
    private uint _unknown17;
    private bool _timeFrozen;
    private readonly List<MapReveal> _mapReveals = new();
    private readonly List<ObjectTypeList> _objectTypeLists = new();
    private string _musicTrackName;

    public bool Active { get; set; }

    public ref readonly BreezeInfo BreezeInfo => ref _breezeInfo;

    public event EventHandler<ScriptingSystem> OnUpdateFinished;

    public ScriptingSystem(IGame game)
        : base(game)
    {
        CameraFadeOverlay = AddDisposable(new CameraFadeOverlay(game));

        _executionContext = new ScriptExecutionContext(game);

        TickRate = game.Definition.ScriptingTicksPerSecond;

        _specialPowers = new List<ObjectNameAndId>[Player.MaxPlayers];
        for (var i = 0; i < _specialPowers.Length; i++)
        {
            _specialPowers[i] = new List<ObjectNameAndId>();
        }

        _unknownSuperweaponArray = new List<ObjectNameAndId>[Player.MaxPlayers];
        for (var i = 0; i < _unknownSuperweaponArray.Length; i++)
        {
            _unknownSuperweaponArray[i] = [];
        }

        _upgrades = new List<ObjectNameAndId>[Player.MaxPlayers];
        for (var i = 0; i < _upgrades.Length; i++)
        {
            _upgrades[i] = new List<ObjectNameAndId>();
        }

        _sciences = new ScienceSet[Player.MaxPlayers];
        for (var i = 0; i < _sciences.Length; i++)
        {
            _sciences[i] = new ScienceSet();
        }

        _breezeInfo = new BreezeInfo();
        _breezeInfo.Direction = MathF.PI / 3.0f;
        _breezeInfo.DirectionVector.X = MathF.Sin(_breezeInfo.Direction);
        _breezeInfo.DirectionVector.Y = MathF.Cos(_breezeInfo.Direction);
        _breezeInfo.Intensity = 0.07f * MathF.PI / 4.0f;
        _breezeInfo.Lean = 0.07f * MathF.PI / 4.0f;
        _breezeInfo.BreezePeriod = (short)(Game.SageGame.LogicFramesPerSecond() * 5);
        _breezeInfo.Randomness = 0.2f;
        _breezeInfo.BreezeVersion = 0;
    }

    internal override void OnSceneChanging()
    {
        _numFlags = 1;
        _numCounters = 1;
    }

    internal override void OnSceneChanged()
    {

    }

    public Script FindScript(string name)
    {
        if (Game.Scene3D?.PlayerScripts?.ScriptLists == null)
        {
            return null;
        }

        for (var i = 0; i < Game.Scene3D.PlayerScripts.ScriptLists.Length; i++)
        {
            var script = Game.Scene3D.PlayerScripts.ScriptLists[i].FindScript(name);
            if (script != null)
            {
                return script;
            }
        }

        return null;
    }

    public bool GetFlagValue(string name)
    {
        ref var flag = ref GetFlag(name);

        return flag.Value;
    }

    public void SetFlagValue(string name, bool value)
    {
        ref var flag = ref GetFlag(name);

        flag.Value = value;
    }

    private ref ScriptingFlag GetFlag(string name)
    {
        for (var i = 0; i < _flags.Length; i++)
        {
            if (_flags[i].Name == name)
            {
                return ref _flags[i];
            }
        }

        if (_numFlags == _flags.Length)
        {
            throw new InvalidOperationException();
        }

        ref var result = ref _flags[_numFlags];

        result.Name = name;
        result.Value = false;

        _numFlags++;

        return ref result;
    }

    public int GetCounterValue(string name)
    {
        ref var counter = ref GetCounter(name);

        return counter.Value;
    }

    public bool HasTimerExpired(string name)
    {
        ref var counter = ref GetCounter(name);

        if (!counter.IsTimer)
        {
            return false;
        }

        return counter.Value == -1;
    }

    public void SetCounterValue(string name, int value)
    {
        ref var counter = ref GetCounter(name);

        counter.Value = value;
    }

    public void AddCounterValue(string name, int valueToAdd)
    {
        ref var counter = ref GetCounter(name);

        counter.Value += valueToAdd;
    }

    public void SubtractCounterValue(string name, int valueToSubtract)
    {
        ref var counter = ref GetCounter(name);

        counter.Value -= valueToSubtract;
    }

    public void SetTimerValue(string name, int value)
    {
        ref var counter = ref GetCounter(name);

        counter.IsTimer = true;
        counter.Value = value;
    }

    private ref ScriptingCounter GetCounter(string name)
    {
        for (var i = 0; i < _counters.Length; i++)
        {
            if (_counters[i].Name == name)
            {
                return ref _counters[i];
            }
        }

        if (_numCounters == _counters.Length)
        {
            throw new InvalidOperationException();
        }

        ref var result = ref _counters[_numCounters];

        result.Name = name;
        result.Value = 0;
        result.IsTimer = false;

        _numCounters++;

        return ref result;
    }

    public void ScriptingTick()
    {
        if (Game.Scene3D?.PlayerScripts?.ScriptLists == null)
        {
            return;
        }

        if (!Active)
        {
            return;
        }

        foreach (var playerScripts in Game.Scene3D.PlayerScripts.ScriptLists)
        {
            playerScripts?.Execute(_executionContext);
        }

        OnUpdateFinished?.Invoke(this, this);

        for (var i = 0; i < _numCounters; i++)
        {
            ref var counter = ref _counters[i];

            if (counter.IsTimer && counter.Value > -1)
            {
                counter.Value -= 1;
            }
        }

        UpdateCameraFadeOverlay();
    }

    // For unit tests.
    internal bool EvaluateScriptCondition(ScriptCondition condition)
    {
        return ScriptConditions.Evaluate(_executionContext, condition);
    }

    internal void SetCameraFade(CameraFadeType type, float from, float to, uint framesIncrease, uint framesHold, uint framesDecrease)
    {
        CameraFadeOverlay.FadeType = type;
        CameraFadeOverlay.From = from;
        CameraFadeOverlay.To = to;
        CameraFadeOverlay.FramesIncrease = framesIncrease;
        CameraFadeOverlay.FramesHold = framesHold;
        CameraFadeOverlay.FramesDecrease = framesDecrease;

        CameraFadeOverlay.CurrentFrame = 0;
    }

    private void UpdateCameraFadeOverlay()
    {
        CameraFadeOverlay.Update();
    }

    public void TransferObjectName(string unitName, GameObject newObject)
    {
        // TODO(Port): Port this from ScriptEngine::transferObjectName().
    }

    // Despite the name, this does in fact execute several actions.
    // This is because in C++, ScriptAction (like most classes in Generals) is a node in an intrusive linked list.
    // There is also ExecutionActions, which this method (should) call.
    public void ExecuteAction(ScriptAction[] actions)
    {
        // TODO(Port): Port this from ScriptEngine::executeAction().
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(5);

        reader.PersistList(
            _sequentialScripts,
            static (StatePersister persister, ref SequentialScript item) =>
            {
                item ??= new SequentialScript();
                persister.PersistObjectValue(item);
            });

        reader.PersistUInt16(ref _numCounters);

        reader.BeginArray("Counters");
        for (var i = 0; i < _numCounters; i++)
        {
            ref var counter = ref _counters[i];

            reader.BeginObject();

            reader.PersistInt32(ref counter.Value, "Value");
            reader.PersistAsciiString(ref counter.Name, "Name");
            reader.PersistBoolean(ref counter.IsTimer, "IsTimer");

            reader.EndObject();
        }
        reader.EndArray();

        var numTimersAndCounters2 = (uint)_numCounters;
        reader.PersistUInt32(ref numTimersAndCounters2);
        if (numTimersAndCounters2 != _numCounters)
        {
            throw new InvalidStateException();
        }

        reader.PersistUInt16(ref _numFlags);

        reader.BeginArray("Flags");
        for (var i = 0; i < _numFlags; i++)
        {
            ref var flag = ref _flags[i];

            reader.BeginObject();

            reader.PersistBoolean(ref flag.Value);
            reader.PersistAsciiString(ref flag.Name);

            reader.EndObject();
        }
        reader.EndArray();

        var numFlags2 = (uint)_numFlags;
        reader.PersistUInt32(ref numFlags2);
        if (numFlags2 != _numFlags)
        {
            throw new InvalidStateException();
        }

        reader.PersistList(
            _attackPriorities,
            static (StatePersister persister, ref AttackPriority item) =>
            {
                item ??= new AttackPriority();
                persister.PersistObjectValue(item);
            });

        var numAttackPrioritySets2 = (uint)_attackPriorities.Count;
        reader.PersistUInt32(ref numAttackPrioritySets2);
        if (numAttackPrioritySets2 != _attackPriorities.Count)
        {
            throw new InvalidStateException();
        }

        var unknown1 = -1;
        reader.PersistInt32(ref unknown1);
        if (unknown1 != -1)
        {
            throw new InvalidStateException();
        }

        var unknown2 = -1;
        reader.PersistInt32(ref unknown2);
        if (unknown2 != -1)
        {
            throw new InvalidStateException();
        }

        reader.PersistList(
            _unknownSomethings,
            static (StatePersister persister, ref ObjectNameAndId item) =>
            {
                persister.PersistObjectValue(ref item);
            });

        reader.SkipUnknownBytes(1);

        reader.PersistObject(CameraFadeOverlay);

        reader.BeginArray("UnknownArray");
        for (var i = 0; i < 4; i++)
        {
            reader.BeginObject();

            reader.PersistVersion(1);

            reader.SkipUnknownBytes(2);

            reader.EndObject();
        }
        reader.EndArray();

        reader.PersistArrayWithUInt16Length(_specialPowers, ObjectNameAndIdListPersister);

        ushort numUnknown1Sets = 0;
        reader.PersistUInt16(ref numUnknown1Sets);

        for (var i = 0; i < numUnknown1Sets; i++)
        {
            reader.PersistVersion(1);

            reader.SkipUnknownBytes(2);
        }

        reader.PersistArrayWithUInt16Length(_unknownSuperweaponArray, ObjectNameAndIdListPersister);

        reader.PersistArrayWithUInt16Length(_upgrades, ObjectNameAndIdListPersister);

        reader.PersistArrayWithUInt16Length(
            _sciences,
            static (StatePersister persister, ref ScienceSet item) =>
            {
                persister.PersistObjectValue(item);
            });

        byte unknown14_1 = 1;
        reader.PersistByte(ref unknown14_1);
        if (unknown14_1 != 1)
        {
            throw new InvalidStateException();
        }

        reader.SkipUnknownBytes(2);

        reader.PersistObject(ref _breezeInfo);

        reader.PersistUInt32(ref _unknown17);
        if (_unknown17 != 0 && _unknown17 != 1 && _unknown17 != 2)
        {
            throw new InvalidStateException();
        }

        reader.PersistBoolean(ref _timeFrozen);

        reader.PersistList(
            _mapReveals,
            static (StatePersister persister, ref MapReveal item) =>
            {
                persister.BeginObject();

                persister.PersistAsciiString(ref item.Name);
                persister.PersistAsciiString(ref item.Waypoint);
                persister.PersistSingle(ref item.Radius);
                persister.PersistAsciiString(ref item.Player);

                persister.EndObject();
            });

        reader.PersistList(
            _objectTypeLists,
            static (StatePersister persister, ref ObjectTypeList item) =>
            {
                item ??= new ObjectTypeList();
                persister.PersistObjectValue(item);
            });

        byte unknown20 = 1;
        reader.PersistByte(ref unknown20);
        if (unknown20 != 1)
        {
            throw new InvalidStateException();
        }

        reader.PersistAsciiString(ref _musicTrackName);

        reader.SkipUnknownBytes(1);
    }

    private static void ObjectNameAndIdListPersister(StatePersister persister, ref List<ObjectNameAndId> item) => persister.PersistObjectNameAndIdList(item);

    internal void Dump(StringBuilder sb)
    {
        sb.AppendLine("Counters:");

        foreach (var kv in _counters)
        {
            sb.AppendFormat("  {0}: {1} (IsTimer: {2})\n", kv.Name, kv.Value, kv.IsTimer);
        }

        sb.AppendLine("Flags:");

        foreach (var kv in _flags)
        {
            sb.AppendFormat("  {0}: {1}\n", kv.Name, kv.Value);
        }
    }

    /// <summary>
    /// If the object is named and has a specified topple direction, adjust
    /// topple direction to reflect it.
    /// </summary>
    public void AdjustToppleDirection(GameObject gameObject, in Vector3 direction)
    {
        // TODO(Port): Port this from ScriptEngine::adjustToppleDirection().
    }
}

internal struct ScriptingFlag
{
    public bool Value;
    public string Name;
}

internal struct ScriptingCounter
{
    public int Value;
    public string Name;
    public bool IsTimer;
}

internal struct MapReveal
{
    public string Name;
    public string Waypoint;
    public float Radius;
    public string Player;
}

public struct BreezeInfo : IPersistableObject
{
    /// <summary>
    /// Direction of the breeze in radians. 0 == +x direction.
    /// </summary>
    public float Direction;

    /// <summary>
    /// Sin/cos of direction, for efficiency.
    /// </summary>
    public Vector2 DirectionVector;

    /// <summary>
    /// How far to sway back and forth in radians. 0 == none.
    /// </summary>
    public float Intensity;

    /// <summary>
    /// How far to lean with the wind in radians. 0 == none.
    /// </summary>
    public float Lean;

    /// <summary>
    /// Randomness. 0 == perfectly uniform, 1 == +- up to 50% randomly.
    /// </summary>
    public float Randomness;

    /// <summary>
    /// How many frames it takes to sway forward & back.
    /// </summary>
    public short BreezePeriod;

    /// <summary>
    /// Incremented each time the settings are updated.
    /// </summary>
    public short BreezeVersion;

    public void Persist(StatePersister persister)
    {
        persister.PersistSingle(ref Direction);
        persister.PersistVector2(ref DirectionVector);
        persister.PersistSingle(ref Intensity);
        persister.PersistSingle(ref Lean);
        persister.PersistSingle(ref Randomness);
        persister.PersistInt16(ref BreezePeriod);
        persister.PersistInt16(ref BreezeVersion);
    }
}

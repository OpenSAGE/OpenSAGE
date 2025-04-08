using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Logic.Map;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic;

internal sealed class GameLogic : DisposableBase, IGameObjectCollection, IPersistableObject
{
    private readonly IGame _game;
    private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;

    private readonly List<GameObject> _objects = new();

    private readonly Dictionary<string, GameObject> _nameLookup = new();

    private readonly List<GameObject> _destroyList = new();

    private readonly SleepyUpdateList _sleepyUpdates = new();
    private UpdateModule _currentUpdateModule;

    private readonly Dictionary<string, ObjectBuildableType> _techTreeOverrides = new();
    private readonly List<string> _commandSetNamesPrefixedWithCommandButtonIndex = new();

    private LogicFrame _currentFrame;

    public LogicFrame CurrentFrame => _currentFrame;

    private uint _rankLevelLimit;

    // object id -> some frame? unsure if order matters for this collection
    private readonly Dictionary<ObjectId, uint> _structuresBeingSold = [];

    internal uint NextObjectId = 1;

    private readonly List<GameObject> _objectsToIterate = new();
    // TODO: This allocates memory. Don't do this.
    public IEnumerable<GameObject> Objects
    {
        get
        {
            // TODO: We can't return _objects directly because it's possible for new objects to be added
            // during iteration. We should instead create new objects in a pending list, like we do for
            // removed objects.
            _objectsToIterate.Clear();
            foreach (var gameObject in _objects)
            {
                if (gameObject != null)
                {
                    _objectsToIterate.Add(gameObject);
                }
            }
            return _objectsToIterate;
        }
    }

    public readonly IRandom Random;

    public GameLogic(IGame game)
    {
        _game = game;
        _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(game.AssetStore.ObjectDefinitions);

        Random = game.CreateRandom();
    }

    public GameObject CreateObject(ObjectDefinition objectDefinition, Player player)
    {
        if (objectDefinition == null)
        {
            // TODO: Is this ever valid?
            return null;
        }

        var gameObject = AddDisposable(new GameObject(objectDefinition, _game.GameEngine, player));

        gameObject.Id = new ObjectId(NextObjectId++);

        var now = CurrentFrame;
        if (now == LogicFrame.Zero)
        {
            now = new LogicFrame(1);
        }

        foreach (var module in gameObject.BehaviorModules)
        {
            if (module is UpdateModule updateModule)
            {
                var when = updateModule.NextCallFrame;

                // Note that "when" can be zero here for any update module
                // that didn't bother to call SetWakeFrame in its constructor.
                // This is legal.
                if (when == LogicFrame.Zero)
                {
                    updateModule.NextCallFrame = now;
                }

                DebugUtility.AssertCrash(updateModule.NextCallFrame >= now, $"You may not specify a zero initial sleep time for sleepy modules ({updateModule.NextCallFrame} {now})");

                _sleepyUpdates.Add(updateModule);
            }
        }

        foreach (var module in gameObject.BehaviorModules)
        {
            if (module is ICreateModule createModule)
            {
                createModule.OnCreate();
            }
        }

        _game.Scene3D.Quadtree?.Insert(gameObject);
        _game.Scene3D.Radar?.AddGameObject(gameObject);
        _game.PartitionCellManager.OnObjectAdded(gameObject);

        return gameObject;
    }

    internal void OnObjectIdChanged(GameObject gameObject, ObjectId oldObjectId)
    {
        if (oldObjectId.IsValid)
        {
            SetObject(oldObjectId, null);
        }

        SetObject(gameObject.Id, gameObject);
    }

    private void SetObject(ObjectId objectId, GameObject gameObject)
    {
        while (_objects.Count <= objectId.Index)
        {
            _objects.Add(null);
        }
        _objects[(int)objectId.Index] = gameObject;
    }

    public GameObject GetObjectById(ObjectId id)
    {
        return _objects[(int)id.Index];
    }

    public bool TryGetObjectByName(string name, out GameObject gameObject)
    {
        return _nameLookup.TryGetValue(name, out gameObject);
    }

    public void AddNameLookup(GameObject gameObject)
    {
        _nameLookup[gameObject.Name ?? throw new ArgumentException("Cannot add lookup for unnamed object.")] = gameObject;
    }

    private void DestroyAllObjectsNow()
    {
        foreach (var gameObject in _objects)
        {
            if (gameObject != null)
            {
                DestroyObject(gameObject);
            }
        }

        DeleteDestroyed();
    }

    public void DestroyObject(GameObject gameObject)
    {
        _destroyList.Add(gameObject);
    }

    public void DeleteDestroyed()
    {
        foreach (var gameObject in _destroyList)
        {
            _game.Scene3D?.Quadtree.Remove(gameObject);
            _game.Scene3D?.Radar.RemoveGameObject(gameObject);
            gameObject.PartitionObject.Remove();

            gameObject.Drawable.Destroy();

            gameObject.OnDestroy();

            if (gameObject.Name != null)
            {
                _nameLookup.Remove(gameObject.Name);
            }

            foreach (var updateModule in gameObject.FindBehaviors<UpdateModule>())
            {
                _sleepyUpdates.Remove(updateModule.IndexInLogic);

                DebugUtility.AssertCrash(updateModule.IndexInLogic == -1, "Hmm, expected index to be -1 here");
            }

            gameObject.Dispose();

            RemoveToDispose(gameObject);

            _objects[(int)gameObject.Id.Index] = null;
        }

        _destroyList.Clear();
    }

    public void Reset()
    {
        DestroyAllObjectsNow();

        NextObjectId = 1;
    }

    public void DeselectObject(GameObject obj, PlayerMaskType playerMask, bool affectClient)
    {
        // TODO(Port): Implement this.
    }

    public void Update()
    {
        var now = _currentFrame;

        while (_sleepyUpdates.Count > 0)
        {
            var updateModule = _sleepyUpdates.Peek();

            if (updateModule.NextCallFrame > now)
            {
                // We're done. Everything else is sleeping.
                // Break from the loop _before_ we pop this item off.
                break;
            }

            // Default, if it is disabled.
            var sleepLength = UpdateSleepTime.None;

            var disabledMask = updateModule.ParentGameObject.DisabledFlags;
            if (!disabledMask.AnyBitSet || disabledMask.Intersects(updateModule.DisabledTypesToProcess))
            {
                _currentUpdateModule = updateModule;

                sleepLength = updateModule.Update();

                DebugUtility.AssertCrash(sleepLength.FrameSpan > LogicFrameSpan.Zero, "You may not return 0 from an update");
                if (sleepLength.FrameSpan < LogicFrameSpan.One)
                {
                    sleepLength = UpdateSleepTime.None;
                }

                _currentUpdateModule = null;
            }

            // Defer it till the next frame and re-push it.
            updateModule.NextCallFrame = now + sleepLength.FrameSpan;
            _sleepyUpdates.Rebalance(0);
        }

        _sleepyUpdates.Validate();

        _currentFrame++;
    }

    // Sleepy update stuff.

    internal void AwakenUpdateModule(GameObject gameObject, UpdateModule updateModule, LogicFrame whenToWakeUp)
    {
        var now = CurrentFrame;
        DebugUtility.AssertCrash(whenToWakeUp >= now, "SetWakeFrame frame is in the past... are you sure this is what you want?");

        if (updateModule == _currentUpdateModule)
        {
            DebugUtility.Crash("You should not call SetWakeFrame() from inside your Update(), because it will be ignored, in favor of the return code from Update");
            return;
        }

        if (whenToWakeUp == updateModule.NextCallFrame)
        {
            // That was easy.
            return;
        }

        if (now > LogicFrame.Zero && updateModule.NextCallFrame == now && whenToWakeUp == now + LogicFrameSpan.One)
        {
            // Subtle but important detail: if we are already awake, and someone
            // calls SetWakeFrame(UpdateSleepTime.None), we don't want to reset
            // our wake frame, since that would prevent us from getting called
            // THIS frame. Since UpdateSleepTime.None really means "wake up as
            // soon as possible", we don't want to change our status if we are
            // already awake.
            return;
        }

        var index = updateModule.IndexInLogic;
        if (_objects.Contains(gameObject))
        {
            if (index < 0 || index >= _sleepyUpdates.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Sleepy update module illegal index");
            }

            if (_sleepyUpdates[index] != updateModule)
            {
                throw new InvalidOperationException("Sleepy update module index mismatch");
            }

            // Update the value.
            updateModule.NextCallFrame = whenToWakeUp;

            // Rebalance.
            _sleepyUpdates.Rebalance(index);

            // Validate.
            _sleepyUpdates.Validate();
        }
        else
        {
            if (index != -1)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Sleepy update module index mismatch");
            }

            // This can happen if stuff happens during object initialization.
            // Fortunately, it's easy to deal with.
            updateModule.NextCallFrame = whenToWakeUp;
        }
    }

    internal void DrawUpdateModulesDiagnosticTable()
    {
        _sleepyUpdates.DrawDiagnosticTable();
    }

    public void Persist(StatePersister reader)
    {
        var version = reader.PersistVersion(10);

        var currentFrame = _currentFrame.Value;
        reader.PersistUInt32(ref currentFrame);
        _currentFrame = new LogicFrame(currentFrame);

        reader.PersistObject(_objectDefinitionLookupTable, "ObjectDefinitions");

        var objectsCount = (uint)_objects.Count;
        reader.PersistUInt32(ref objectsCount);

        reader.BeginArray("Objects");
        if (reader.Mode == StatePersistMode.Read)
        {
            DestroyAllObjectsNow();
            _objects.EnsureCapacity((int)objectsCount);

            for (var i = 0; i < objectsCount; i++)
            {
                reader.BeginObject();

                ushort objectDefinitionId = 0;
                reader.PersistUInt16(ref objectDefinitionId);
                var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                var gameObject = CreateObject(objectDefinition, null);

                reader.BeginSegment(objectDefinition.Name);

                reader.PersistObject(gameObject, "Object");

                NextObjectId = Math.Max(NextObjectId, gameObject.Id.Index + 1);

                reader.EndSegment();

                reader.EndObject();
            }
        }
        else
        {
            foreach (var gameObject in _objects)
            {
                if (gameObject == null)
                {
                    continue;
                }

                reader.BeginObject();

                var objectDefinitionId = _objectDefinitionLookupTable.GetId(gameObject.Definition);
                reader.PersistUInt16(ref objectDefinitionId);

                reader.BeginSegment(gameObject.Definition.Name);

                reader.PersistObject(gameObject, "Object");

                reader.EndSegment();

                reader.EndObject();
            }
        }
        reader.EndArray();

        // Don't know why this is duplicated here. It's also loaded by a top-level .sav chunk.
        reader.PersistObject(reader.Game.CampaignManager, "CampaignManager");

        var unknown1 = true;
        reader.PersistBoolean(ref unknown1);
        if (!unknown1)
        {
            throw new InvalidStateException();
        }

        reader.SkipUnknownBytes(2);

        var unknown1_1 = true;
        reader.PersistBoolean(ref unknown1_1);
        if (!unknown1_1)
        {
            throw new InvalidStateException();
        }

        reader.PersistFixedLengthListWithUInt32Count(
            _game.Scene3D.MapFile.PolygonTriggers.Triggers,
            static (StatePersister persister, ref PolygonTrigger item) =>
            {
                persister.BeginObject();

                var id = item.TriggerId;
                persister.PersistUInt32(ref id);

                if (id != item.TriggerId)
                {
                    throw new InvalidStateException();
                }

                persister.PersistObject(item);

                persister.EndObject();
            },
            "PolygonTriggers");

        reader.PersistUInt32(ref _rankLevelLimit);

        reader.PersistDictionaryWithUInt32Count(_structuresBeingSold,
            (StatePersister persister, ref ObjectId objectId, ref uint saleFinishedFrameMaybe) =>
            {
                persister.PersistObjectId(ref objectId);
                persister.PersistUInt32(ref saleFinishedFrameMaybe);
            });

        reader.BeginArray("TechTreeOverrides");
        if (reader.Mode == StatePersistMode.Read)
        {
            while (true)
            {
                reader.BeginObject();

                var objectDefinitionName = "";
                reader.PersistAsciiString(ref objectDefinitionName);

                if (objectDefinitionName == "")
                {
                    reader.EndObject();
                    break;
                }

                ObjectBuildableType buildableStatus = default;
                reader.PersistEnum(ref buildableStatus);

                _techTreeOverrides.Add(
                    objectDefinitionName,
                    buildableStatus);

                reader.EndObject();
            }
        }
        else
        {
            foreach (var techTreeOverride in _techTreeOverrides)
            {
                reader.BeginObject();

                var objectDefinitionName = techTreeOverride.Key;
                reader.PersistAsciiString(ref objectDefinitionName);

                var buildableStatus = techTreeOverride.Value;
                reader.PersistEnum(ref buildableStatus);

                reader.EndObject();
            }

            reader.BeginObject();

            var endString = "";
            reader.PersistAsciiString(ref endString, "ObjectDefinitionName");

            reader.EndObject();
        }
        reader.EndArray();

        var unknownBool1 = true;
        reader.PersistBoolean(ref unknownBool1);
        if (!unknownBool1)
        {
            throw new InvalidStateException();
        }

        var unknownBool2 = true;
        reader.PersistBoolean(ref unknownBool2);
        if (!unknownBool2)
        {
            throw new InvalidStateException();
        }

        var unknownBool3 = true;
        reader.PersistBoolean(ref unknownBool3);
        if (!unknownBool3)
        {
            throw new InvalidStateException();
        }

        var unknown3 = uint.MaxValue;
        reader.PersistUInt32(ref unknown3);
        if (unknown3 != uint.MaxValue)
        {
            throw new InvalidStateException();
        }

        // Command button overrides
        reader.BeginArray("CommandButtonOverrides");
        if (reader.Mode == StatePersistMode.Read)
        {
            while (true)
            {
                var commandSetNamePrefixedWithCommandButtonIndex = "";
                reader.PersistAsciiStringValue(ref commandSetNamePrefixedWithCommandButtonIndex);

                if (commandSetNamePrefixedWithCommandButtonIndex == "")
                {
                    break;
                }

                _commandSetNamesPrefixedWithCommandButtonIndex.Add(commandSetNamePrefixedWithCommandButtonIndex);

                reader.SkipUnknownBytes(1);
            }
        }
        else
        {
            foreach (var commandSetName in _commandSetNamesPrefixedWithCommandButtonIndex)
            {
                var commandSetNameCopy = commandSetName;
                reader.PersistAsciiStringValue(ref commandSetNameCopy);

                reader.SkipUnknownBytes(1);
            }

            var endString = "";
            reader.PersistAsciiStringValue(ref endString);
        }
        reader.EndArray();

        reader.SkipUnknownBytes(4);

        if (version >= 10)
        {
            reader.SkipUnknownBytes(2);
        }
    }
}

internal sealed class ObjectDefinitionLookupTable : IPersistableObject
{
    private readonly ScopedAssetCollection<ObjectDefinition> _objectDefinitions;
    private readonly List<ObjectDefinitionLookupEntry> _entries = new();

    public ObjectDefinitionLookupTable(ScopedAssetCollection<ObjectDefinition> objectDefinitions)
    {
        _objectDefinitions = objectDefinitions;
    }

    public ObjectDefinition GetById(ushort id)
    {
        foreach (var entry in _entries)
        {
            if (entry.Id == id)
            {
                return _objectDefinitions.GetByName(entry.Name);
            }
        }

        throw new InvalidOperationException();
    }

    public ushort GetId(ObjectDefinition objectDefinition)
    {
        foreach (var entry in _entries)
        {
            if (entry.Name == objectDefinition.Name)
            {
                return entry.Id;
            }
        }

        var newEntry = new ObjectDefinitionLookupEntry
        {
            Name = objectDefinition.Name,
            Id = (ushort)_entries.Count
        };

        _entries.Add(newEntry);

        return newEntry.Id;
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistListWithUInt32Count(
            _entries,
            static (StatePersister persister, ref ObjectDefinitionLookupEntry item) =>
            {
                persister.PersistObjectValue(ref item);
            });
    }

    private struct ObjectDefinitionLookupEntry : IPersistableObject
    {
        public string Name;
        public ushort Id;

        public void Persist(StatePersister persister)
        {
            persister.PersistAsciiString(ref Name);
            persister.PersistUInt16(ref Id);
        }
    }
}

/// <summary>
/// A sorted tree-based list of sleepy <see cref="UpdateModule"/>s.
/// </summary>
internal sealed class SleepyUpdateList
{
    // TODO: Find a sensible default capacity for this.
    private readonly List<UpdateModule> _inner = new();

    public int Count => _inner.Count;

    public UpdateModule this[int index]
    {
        get => _inner[index];
    }

    public void Rebalance(int index)
    {
        index = RebalanceParent(index);
        RebalanceChild(index);
    }

    private int RebalanceParent(int i)
    {
        DebugUtility.AssertCrash(i >= 0 && i < _inner.Count, "Bad sleepy index");

        var parent = ((i + 1) >> 1) - 1;
        while (parent >= 0 && IsLowerPriority(_inner[parent], _inner[i]))
        {
            var a = _inner[parent];
            var b = _inner[i];

            _inner[i] = a;
            _inner[parent] = b;

            a.IndexInLogic = i;
            b.IndexInLogic = parent;

            i = parent;
            parent = ((parent + 1) >> 1) - 1;
        }

        return i;
    }

    private int RebalanceChild(int i)
    {
        DebugUtility.AssertCrash(i >= 0 && i < _inner.Count, "Bad sleepy index");

        // Our children as index*2 and index*2+1.
        var count = _inner.Count;
        var child = ((i + 1) << 1) - 1;
        while (child < count)
        {
            // Choose the higher-priority of the two children; we must be higher-priority than that.
            if (child < count - 1 && IsLowerPriority(_inner[child], _inner[child + 1]))
            {
                ++child;
            }

            // If we're higher-priority than our children, we're done.
            if (!IsLowerPriority(_inner[i], _inner[child]))
            {
                break;
            }

            // Doh. Swap with the highest-priority child we have.
            var a = _inner[child];
            var b = _inner[i];

            _inner[i] = a;
            _inner[child] = b;

            a.IndexInLogic = i;
            b.IndexInLogic = child;

            i = child;
            child = ((i + 1) << 1) - 1;
        }

        return i;
    }

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="a"/> is lower priority
    /// than <paramref name="b"/>.
    /// </summary>
    private static bool IsLowerPriority(UpdateModule a, UpdateModule b)
    {
        // Remember: lower ordinal value means higher priority.
        // Therefore, higher ordinal value means lower priority.
        return a.Priority > b.Priority;
    }

    public void Add(UpdateModule updateModule)
    {
        DebugUtility.AssertCrash(updateModule != null, "You may not pass null for sleepy update info");

        _inner.Add(updateModule);
        updateModule.IndexInLogic = _inner.Count - 1;

        RebalanceParent(_inner.Count - 1);
    }

    public void Remove(int index)
    {
        DebugUtility.AssertCrash(index >= 0 && index < _inner.Count, "Bad sleepy index");

        // Swap with the final item, toss the final item, then rebalance.
        _inner[index].IndexInLogic = -1;

        var final = _inner.Count - 1;
        if (index < final)
        {
            _inner[index] = _inner[final];
            _inner[index].IndexInLogic = index;
            _inner.RemoveAt(final);
            Rebalance(index);
        }
        else
        {
            _inner.RemoveAt(final);
        }
    }

    /// <summary>
    /// Returns the <see cref="UpdateModule"/> at the front of the list,
    /// but does not modify the list.
    /// </summary>
    /// <returns></returns>
    public UpdateModule Peek()
    {
        var updateModule = _inner[0];

        DebugUtility.AssertCrash(updateModule.IndexInLogic == 0, $"Index mismatch: expected 0, got {updateModule.IndexInLogic}");

        return updateModule;
    }

    // TODO(Port): Call this from GameLogic.LoadPostProcess().
    public void Remake()
    {
        var parent = _inner.Count / 2;
        while (true)
        {
            RebalanceChild(parent);
            if (parent == 0)
            {
                break;
            }
            --parent;
        }

        Validate();
    }

    [Conditional("DEBUG")]
    public void Validate()
    {
        for (var i = 0; i < _inner.Count; i++)
        {
            var updateModule = _inner[i];

            DebugUtility.AssertCrash(updateModule.IndexInLogic == i, $"Index mismatch: expected {i}, got {updateModule.IndexInLogic}");

            var priority = updateModule.Priority;
            if (i > 0)
            {
                var i0 = ((i + 1) / 2) - 1;
                var priority0 = _inner[i0].Priority;
                DebugUtility.AssertCrash(priority >= priority0, "Sleepy updates are broken");
            }

            var i1 = (2 * (i + 1)) - 1;
            var i2 = 2 * (i + 1);
            if (i1 < _inner.Count)
            {
                var priority1 = _inner[i1].Priority;
                DebugUtility.AssertCrash(priority <= priority1, "Sleepy updates are broken");
            }
            if (i2 < _inner.Count)
            {
                var priority2 = _inner[i2].Priority;
                DebugUtility.AssertCrash(priority <= priority2, "Sleepy updates are broken");
            }
        }
    }

    private readonly List<UpdateModule> _sortedList = new();

    internal void DrawDiagnosticTable()
    {
        ImGui.Text($"Total update modules: {_inner.Count}");
        ImGui.Separator();

        if (ImGui.BeginTable("update-modules", 4, ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Module");
            ImGui.TableSetupColumn("Object");
            ImGui.TableSetupColumn("Next Frame");
            ImGui.TableSetupColumn("Phase");
            ImGui.TableHeadersRow();

            // Unfortunately, we can't directly get an ordered list out of a priority queue.
            // So instead we copy the whole thing into a simple list and sort that.
            // This is super slow.
            _sortedList.Clear();
            _sortedList.AddRange(_inner);
            _sortedList.Sort(static (x, y) => x.Priority.CompareTo(y.Priority));
            foreach (var updateModule in _sortedList)
            {
                DrawDiagnosticTableRow(updateModule);
            }

            ImGui.EndTable();
        }
    }

    private static void DrawDiagnosticTableRow(UpdateModule updateModule)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.Text(updateModule.GetType().Name);

        ImGui.TableNextColumn();
        ImGui.Text(updateModule.ParentGameObject.Name ?? updateModule.ParentGameObject.Definition.Name);

        ImGui.TableNextColumn();
        if (updateModule.NextCallFrame.Value == UpdateSleepTime.SleepForever)
        {
            ImGui.Text($"Sleep forever");
        }
        else
        {
            ImGui.Text(updateModule.NextCallFrame.Value.ToString());
        }

        ImGui.TableNextColumn();
        ImGui.Text(((int)updateModule.NextCallPhase).ToString());
    }
}

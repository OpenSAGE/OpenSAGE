using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;

namespace OpenSage.Logic
{
    internal sealed class GameLogic : DisposableBase, IPersistableObject
    {
        private readonly Game _game;
        private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;

        private readonly List<GameObject> _objects = new();

        private readonly Dictionary<string, GameObject> _nameLookup = new();

        private readonly List<GameObject> _destroyList = new();

        private readonly Dictionary<string, ObjectBuildableType> _techTreeOverrides = new();
        private readonly List<string> _commandSetNamesPrefixedWithCommandButtonIndex = new();

        private uint _currentFrame;

        private uint _rankLevelLimit;

        internal uint NextObjectId = 1;

        // TODO: This allocates memory. Don't do this.
        public IEnumerable<GameObject> Objects
        {
            get
            {
                foreach (var gameObject in _objects)
                {
                    if (gameObject != null)
                    {
                        yield return gameObject;
                    }
                }
            }
        }

        public GameLogic(Game game)
        {
            _game = game;
            _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(game.AssetStore.ObjectDefinitions);
        }

        public GameObject CreateObject(ObjectDefinition objectDefinition, Player player)
        {
            if (objectDefinition == null)
            {
                // TODO: Is this ever valid?
                return null;
            }

            var gameObject = AddDisposable(new GameObject(objectDefinition, _game.Scene3D.GameContext, player));

            gameObject.ID = NextObjectId++;

            _game.Scene3D.Quadtree?.Insert(gameObject);
            _game.Scene3D.Radar?.AddGameObject(gameObject);

            return gameObject;
        }

        internal void OnObjectIdChanged(GameObject gameObject, uint oldObjectId)
        {
            if (oldObjectId != 0)
            {
                SetObject(oldObjectId, null);
            }

            SetObject(gameObject.ID, gameObject);
        }

        private void SetObject(uint objectId, GameObject gameObject)
        {
            while (_objects.Count <= objectId)
            {
                _objects.Add(null);
            }
            _objects[(int)objectId] = gameObject;
        }

        public GameObject GetObjectById(uint id)
        {
            return _objects[(int)id];
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

        internal void DeleteDestroyed()
        {
            foreach (var gameObject in _destroyList)
            {
                _game.Scene3D.Quadtree.Remove(gameObject);
                _game.Scene3D.Radar.RemoveGameObject(gameObject);

                gameObject.Drawable.Destroy();

                if (gameObject.Name != null)
                {
                    _nameLookup.Remove(gameObject.Name);
                }

                gameObject.Dispose();

                RemoveToDispose(gameObject);

                _objects[(int)gameObject.ID] = null;
            }

            _destroyList.Clear();
        }

        public void Reset()
        {
            DestroyAllObjectsNow();

            NextObjectId = 1;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(9);

            reader.PersistUInt32(ref _currentFrame);
            reader.PersistObject(_objectDefinitionLookupTable, "ObjectDefinitions");

            var objectsCount = (uint)_objects.Count;
            reader.PersistUInt32(ref objectsCount);

            reader.BeginArray("Objects");
            if (reader.Mode == StatePersistMode.Read)
            {
                _objects.Clear();
                _objects.Capacity = (int)objectsCount;

                for (var i = 0; i < objectsCount; i++)
                {
                    reader.BeginObject();

                    ushort objectDefinitionId = 0;
                    reader.PersistUInt16(ref objectDefinitionId);
                    var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                    var gameObject = CreateObject(objectDefinition, null);

                    reader.BeginSegment(objectDefinition.Name);

                    reader.PersistObject(gameObject, "Object");

                    NextObjectId = Math.Max(NextObjectId, gameObject.ID + 1);

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

            reader.PersistArrayWithUInt32Length(
                _game.Scene3D.MapFile.PolygonTriggers.Triggers,
                static (StatePersister persister, ref PolygonTrigger item) =>
                {
                    persister.BeginObject();

                    var id = item.UniqueId;
                    persister.PersistUInt32(ref id);

                    if (id != item.UniqueId)
                    {
                        throw new InvalidStateException();
                    }

                    persister.PersistObject(item);

                    persister.EndObject();
                },
                "PolygonTriggers");

            reader.PersistUInt32(ref _rankLevelLimit);

            reader.SkipUnknownBytes(4);

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
}

using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;

namespace OpenSage.Logic
{
    internal sealed class GameLogic
    {
        private readonly Scene3D _scene3D;
        private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;
        private readonly List<GameObject> _objects = new();
        private readonly Dictionary<string, ObjectBuildableType> _techTreeOverrides = new();
        private readonly List<string> _commandSetNamesPrefixedWithCommandButtonIndex = new();

        private uint _currentFrame;

        private uint _rankLevelLimit;

        internal uint NextObjectId;

        public GameLogic(Scene3D scene3D)
        {
            _scene3D = scene3D;
            _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(scene3D.AssetLoadContext.AssetStore.ObjectDefinitions);
        }

        public GameObject GetObjectById(uint id)
        {
            return _objects[(int)id];
        }

        public void Load(StatePersister reader)
        {
            reader.PersistVersion(9);

            reader.PersistUInt32("currentFrame", ref _currentFrame);

            _objectDefinitionLookupTable.Load(reader);

            var gameObjectsCount = (uint)_objects.Count;
            reader.PersistUInt32("GameObjectsCount", ref gameObjectsCount);

            _objects.Clear();
            _objects.Capacity = (int)gameObjectsCount;

            for (var i = 0; i < gameObjectsCount; i++)
            {
                ushort objectDefinitionId = 0;
                reader.PersistUInt16(ref objectDefinitionId);
                var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                var gameObject = _scene3D.GameObjects.Add(objectDefinition, _scene3D.LocalPlayer);

                reader.BeginSegment(objectDefinition.Name);

                gameObject.Load(reader);

                while (_objects.Count <= gameObject.ID)
                {
                    _objects.Add(null);
                }
                _objects[(int)gameObject.ID] = gameObject;

                reader.EndSegment();
            }

            // Don't know why this is duplicated here. It's also loaded by a top-level .sav chunk.
            var campaignManager = new CampaignManager();
            campaignManager.Load(reader);

            var unknown1 = true;
            reader.PersistBoolean("Unknown1", ref unknown1);
            if (!unknown1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(2);

            var unknown1_1 = true;
            reader.PersistBoolean("Unknown1_1", ref unknown1_1);
            if (!unknown1_1)
            {
                throw new InvalidStateException();
            }

            reader.PersistArrayWithUInt32Length("PolygonTriggers", _scene3D.MapFile.PolygonTriggers.Triggers, static (StatePersister persister, ref PolygonTrigger item) =>
            {
                persister.BeginObject();

                var id = item.UniqueId;
                persister.PersistUInt32("Id", ref id);

                if (id != item.UniqueId)
                {
                    throw new InvalidStateException();
                }

                persister.PersistObject("Value", item);

                persister.EndObject();
            });

            reader.PersistUInt32("RankLevelLimit", ref _rankLevelLimit);

            reader.SkipUnknownBytes(4);

            while (true)
            {
                var objectDefinitionName = "";
                reader.PersistAsciiString("ObjectDefinitionName", ref objectDefinitionName);

                if (objectDefinitionName == "")
                {
                    break;
                }

                ObjectBuildableType buildableStatus = default;
                reader.PersistEnum(ref buildableStatus);

                _techTreeOverrides.Add(
                    objectDefinitionName,
                    buildableStatus);
            }

            var unknownBool1 = true;
            reader.PersistBoolean("UnknownBool1", ref unknownBool1);
            if (!unknownBool1)
            {
                throw new InvalidStateException();
            }

            var unknownBool2 = true;
            reader.PersistBoolean("UnknownBool2", ref unknownBool2);
            if (!unknownBool2)
            {
                throw new InvalidStateException();
            }

            var unknownBool3 = true;
            reader.PersistBoolean("UnknownBool3", ref unknownBool3);
            if (!unknownBool3)
            {
                throw new InvalidStateException();
            }

            var unknown3 = uint.MaxValue;
            reader.PersistUInt32("Unknown3", ref unknown3);
            if (unknown3 != uint.MaxValue)
            {
                throw new InvalidStateException();
            }

            // Command button overrides
            while (true)
            {
                var commandSetNamePrefixedWithCommandButtonIndex = "";
                reader.PersistAsciiString("CommandSetNamePrefixedWithCommandButtonIndex", ref commandSetNamePrefixedWithCommandButtonIndex);

                if (commandSetNamePrefixedWithCommandButtonIndex == "")
                {
                    break;
                }

                _commandSetNamesPrefixedWithCommandButtonIndex.Add(commandSetNamePrefixedWithCommandButtonIndex);

                reader.SkipUnknownBytes(1);
            }

            reader.SkipUnknownBytes(4);
        }
    }

    internal sealed class ObjectDefinitionLookupTable
    {
        private readonly ScopedAssetCollection<ObjectDefinition> _objectDefinitions;
        private readonly Dictionary<ushort, string> _nameLookup;

        public ObjectDefinitionLookupTable(ScopedAssetCollection<ObjectDefinition> objectDefinitions)
        {
            _objectDefinitions = objectDefinitions;
            _nameLookup = new Dictionary<ushort, string>();
        }

        public ObjectDefinition GetById(ushort id)
        {
            if (_nameLookup.TryGetValue(id, out var objectDefinitionName))
            {
                return _objectDefinitions.GetByName(objectDefinitionName);
            }

            throw new InvalidOperationException();
        }

        public void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            _nameLookup.Clear();

            var count = (uint)_nameLookup.Count;
            reader.PersistUInt32("Count", ref count);

            for (var i = 0; i < count; i++)
            {
                var name = "";
                reader.PersistAsciiString("Name", ref name);

                ushort id = 0;
                reader.PersistUInt16(ref id);

                _nameLookup.Add(id, name);
            }
        }
    }
}

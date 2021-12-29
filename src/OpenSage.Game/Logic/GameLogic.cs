using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Content;
using OpenSage.Data.Sav;
using OpenSage.Logic.Object;

namespace OpenSage.Logic
{
    internal sealed class GameLogic
    {
        private readonly Scene3D _scene3D;
        private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;
        private readonly List<GameObject> _objects;
        private readonly Dictionary<string, ObjectBuildableType> _techTreeOverrides;

        private uint _currentFrame;

        private uint _rankLevelLimit;

        internal uint NextObjectId;

        public GameLogic(Scene3D scene3D)
        {
            _scene3D = scene3D;
            _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(scene3D.AssetLoadContext.AssetStore.ObjectDefinitions);
            _objects = new List<GameObject>();

            _techTreeOverrides = new Dictionary<string, ObjectBuildableType>();
        }

        public GameObject GetObjectById(uint id)
        {
            return _objects[(int)id];
        }

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(9);

            _currentFrame = reader.ReadUInt32();

            _objectDefinitionLookupTable.Load(reader);

            var gameObjectsCount = reader.ReadUInt32();

            _objects.Clear();
            _objects.Capacity = (int)gameObjectsCount;

            for (var i = 0; i < gameObjectsCount; i++)
            {
                ushort objectDefinitionId = 0;
                reader.ReadUInt16(ref objectDefinitionId);
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

            var unknown1 = reader.ReadBoolean();
            if (!unknown1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(2);

            var unknown1_1 = reader.ReadBoolean();
            if (!unknown1_1)
            {
                throw new InvalidStateException();
            }

            var numPolygonTriggers = reader.ReadUInt32();
            if (numPolygonTriggers != _scene3D.MapFile.PolygonTriggers.Triggers.Length)
            {
                throw new InvalidStateException();
            }
            for (var i = 0; i < numPolygonTriggers; i++)
            {
                var id = reader.ReadUInt32();
                var polygonTrigger = _scene3D.MapFile.PolygonTriggers.GetPolygonTriggerById(id);
                polygonTrigger.Load(reader);
            }

            _rankLevelLimit = reader.ReadUInt32();

            reader.SkipUnknownBytes(4);

            while (true)
            {
                var objectDefinitionName = reader.ReadAsciiString();
                if (objectDefinitionName == "")
                {
                    break;
                }

                ObjectBuildableType buildableStatus = default;
                reader.ReadEnum(ref buildableStatus);

                _techTreeOverrides.Add(
                    objectDefinitionName,
                    buildableStatus);
            }

            if (!reader.ReadBoolean())
            {
                throw new InvalidStateException();
            }

            if (!reader.ReadBoolean())
            {
                throw new InvalidStateException();
            }

            if (!reader.ReadBoolean())
            {
                throw new InvalidStateException();
            }

            var unknown3 = reader.ReadUInt32();
            if (unknown3 != uint.MaxValue)
            {
                throw new InvalidStateException();
            }

            // Command button overrides
            while (true)
            {
                var commandSetNamePrefixedWithCommandButtonIndex = reader.ReadAsciiString();
                if (commandSetNamePrefixedWithCommandButtonIndex == "")
                {
                    break;
                }

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

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            _nameLookup.Clear();

            var count = reader.ReadUInt32();
            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadAsciiString();

                ushort id = 0;
                reader.ReadUInt16(ref id);

                _nameLookup.Add(id, name);
            }
        }
    }
}

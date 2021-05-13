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

        public GameLogic(Scene3D scene3D)
        {
            _scene3D = scene3D;
            _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(scene3D.AssetLoadContext.AssetStore.ObjectDefinitions);
            _objects = new List<GameObject>();
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
                var objectDefinitionId = reader.ReadUInt16();
                var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                var gameObject = _scene3D.GameObjects.Add(objectDefinition, _scene3D.LocalPlayer);

                reader.BeginSegment();

                gameObject.Load(reader);

                while (_objects.Count <= gameObject.ID)
                {
                    _objects.Add(null);
                }
                _objects[(int)gameObject.ID] = gameObject;

                reader.EndSegment();
            }

            reader.ReadByte(); // 3

            var sideName = reader.ReadAsciiString();
            var missionName = reader.ReadAsciiString();

            reader.__Skip(12);

            var numPolygonTriggers = reader.ReadUInt32();
            if (numPolygonTriggers != _scene3D.MapFile.PolygonTriggers.Triggers.Length)
            {
                throw new InvalidDataException();
            }
            for (var i = 0; i < numPolygonTriggers; i++)
            {
                var id = reader.ReadUInt32();
                var polygonTrigger = _scene3D.MapFile.PolygonTriggers.GetPolygonTriggerById(id);
                polygonTrigger.Load(reader);
            }

            _rankLevelLimit = reader.ReadUInt32();

            var unknown2 = reader.ReadUInt32();
            if (unknown2 != 0)
            {
                throw new InvalidDataException();
            }

            while (true)
            {
                var objectDefinitionName = reader.ReadAsciiString();
                if (objectDefinitionName == "")
                {
                    break;
                }

                var buildableStatus = reader.ReadEnum<ObjectBuildableType>();

                _techTreeOverrides.Add(
                    objectDefinitionName,
                    buildableStatus);
            }

            if (!reader.ReadBoolean())
            {
                throw new InvalidDataException();
            }

            if (!reader.ReadBoolean())
            {
                throw new InvalidDataException();
            }

            if (!reader.ReadBoolean())
            {
                throw new InvalidDataException();
            }

            var unknown3 = reader.ReadUInt32();
            if (unknown3 != uint.MaxValue)
            {
                throw new InvalidDataException();
            }

            // Command button overrides
            while (true)
            {
                var commandSetNamePrefixedWithCommandButtonIndex = reader.ReadAsciiString();
                if (commandSetNamePrefixedWithCommandButtonIndex == "")
                {
                    break;
                }

                var unknownBool1 = reader.ReadBoolean();
                if (unknownBool1)
                {
                    throw new InvalidDataException();
                }
            }

            var unknown4 = reader.ReadUInt32();
            if (unknown4 != 0)
            {
                throw new InvalidDataException();
            }
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
                var id = reader.ReadUInt16();

                _nameLookup.Add(id, name);
            }
        }
    }
}

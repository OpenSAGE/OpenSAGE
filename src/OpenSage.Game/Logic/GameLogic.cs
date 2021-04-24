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

        private uint _currentFrame;

        public GameLogic(Scene3D scene3D)
        {
            _scene3D = scene3D;
            _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(scene3D.AssetLoadContext.AssetStore.ObjectDefinitions);
            _objects = new List<GameObject>();
        }

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(9);

            _currentFrame = reader.ReadUInt32();

            _objectDefinitionLookupTable.Load(reader);

            var gameObjectsCount = reader.ReadUInt32();
            for (var i = 0; i < gameObjectsCount; i++)
            {
                var objectDefinitionId = reader.ReadUInt16();
                var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                var gameObject = _scene3D.GameObjects.Add(objectDefinition, _scene3D.LocalPlayer);

                reader.BeginSegment();

                gameObject.Load(reader);

                reader.EndSegment();
            }

            reader.ReadByte(); // 3

            var sideName = reader.ReadAsciiString();
            var missionName = reader.ReadAsciiString();

            reader.__Skip(12);

            var someCount4 = reader.ReadUInt32();
            for (var i = 0; i < someCount4; i++)
            {
                var maybeIndex = reader.ReadUInt32();
                reader.ReadBoolean(); // 1
                var someCount5 = reader.ReadUInt32();
                for (var j = 0; j < someCount5; j++)
                {
                    reader.ReadUInt32();
                    reader.ReadUInt32();
                    reader.ReadUInt32();
                }
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadSingle();
                reader.ReadBoolean();
            }

            reader.ReadUInt32(); // 1000
            reader.ReadUInt32(); // 0
            reader.ReadBoolean(); // 0
            reader.ReadBoolean(); // 1
            reader.ReadBoolean(); // 1
            reader.ReadBoolean(); // 1
            reader.ReadUInt32(); // 0xFFFFFFFF



            reader.ReadUInt32(); // 0
            reader.ReadBoolean(); // 0
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

        private void LoadObjectLookupTable(SaveFileReader reader)
        {
            throw new System.NotImplementedException();
        }
    }
}

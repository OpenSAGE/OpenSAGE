using OpenSage.Data.Sav;
using OpenSage.Logic;

namespace OpenSage.Client
{
    internal sealed class GameClient
    {
        private readonly GameLogic _gameLogic;
        private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;

        private uint _currentFrame;

        public GameClient(Scene3D scene3D, GameLogic gameLogic)
        {
            _gameLogic = gameLogic;
            _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(scene3D.AssetLoadContext.AssetStore.ObjectDefinitions);
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(3);

            _currentFrame = reader.ReadUInt32();

            _objectDefinitionLookupTable.Load(reader);

            var drawablesCount = reader.ReadUInt16();
            for (var i = 0; i < drawablesCount; i++)
            {
                var objectDefinitionId = reader.ReadUInt16();
                var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                reader.BeginSegment();

                var objectID = reader.ReadUInt32();
                var gameObject = _gameLogic.GetObjectById(objectID);

                reader.ReadByte();

                var drawableID = reader.ReadUInt32();

                reader.ReadByte();

                var numModelConditionFlags = reader.ReadUInt32();
                for (var j = 0; j < numModelConditionFlags; j++)
                {
                    var modelConditionFlag = reader.ReadAsciiString();
                }

                var transform = reader.ReadMatrix4x3();

                var unknownBool = reader.ReadBoolean();
                var unknownBool2 = reader.ReadBoolean();
                if (unknownBool)
                {
                    for (var j = 0; j < 9; j++)
                    {
                        reader.ReadSingle();
                    }
                    reader.__Skip(19);
                }

                reader.__Skip(56);

                var unknownBool3 = reader.ReadBoolean();
                if (unknownBool3)
                {
                    for (var j = 0; j < 19; j++)
                    {
                        reader.ReadSingle();
                    }
                }

                reader.__Skip(3);

                var numModules = reader.ReadUInt16();
                for (var moduleIndex = 0; moduleIndex < numModules; moduleIndex++)
                {
                    var moduleTag = reader.ReadAsciiString();
                    reader.BeginSegment();
                    // TODO
                    reader.EndSegment();
                }

                var numClientUpdates = reader.ReadUInt16();
                for (var moduleIndex = 0; moduleIndex < numClientUpdates; moduleIndex++)
                {
                    var moduleTag = reader.ReadAsciiString();
                    reader.BeginSegment();
                    // TODO
                    reader.EndSegment();
                }

                reader.__Skip(81);
            }

            reader.ReadUInt32();
        }
    }
}

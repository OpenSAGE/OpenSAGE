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

                gameObject.Drawable.Load(reader);

                reader.EndSegment();
            }

            reader.ReadUInt32();
        }
    }
}

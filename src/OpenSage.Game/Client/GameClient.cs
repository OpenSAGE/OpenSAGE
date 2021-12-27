using System.Collections.Generic;
using OpenSage.Logic;

namespace OpenSage.Client
{
    internal sealed class GameClient
    {
        private readonly GameLogic _gameLogic;
        private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;
        private readonly Dictionary<uint, Drawable> _drawablesById = new();
        private readonly List<string> _briefingTexts = new();

        private uint _currentFrame;

        internal uint NextDrawableId;

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

                reader.BeginSegment(objectDefinition.Name);

                var objectID = reader.ReadUInt32();
                var gameObject = _gameLogic.GetObjectById(objectID);

                gameObject.Drawable.Load(reader);

                _drawablesById[gameObject.Drawable.DrawableID] = gameObject.Drawable;

                reader.EndSegment();
            }

            var numBriefingTexts = reader.ReadUInt32();
            for (var i = 0; i < numBriefingTexts; i++)
            {
                _briefingTexts.Add(reader.ReadAsciiString());
            }
        }
    }
}

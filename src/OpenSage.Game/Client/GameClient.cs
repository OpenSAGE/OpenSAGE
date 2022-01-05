using System.Collections.Generic;
using OpenSage.Logic;

namespace OpenSage.Client
{
    internal sealed class GameClient
    {
        private readonly GameLogic _gameLogic;
        private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;
        private readonly List<Drawable> _drawables = new();
        private readonly Dictionary<uint, Drawable> _drawablesById = new();
        private readonly List<string> _briefingTexts = new();

        private uint _currentFrame;

        internal uint NextDrawableId;

        public GameClient(Scene3D scene3D, GameLogic gameLogic)
        {
            _gameLogic = gameLogic;
            _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(scene3D.AssetLoadContext.AssetStore.ObjectDefinitions);
        }

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(3);

            reader.PersistUInt32("CurrentFrame", ref _currentFrame);
            reader.PersistObject("ObjectDefinitions", _objectDefinitionLookupTable);

            var drawablesCount = (ushort)_drawables.Count;
            reader.PersistUInt16("DrawablesCount", ref drawablesCount);

            reader.BeginArray("Drawables");
            if (reader.Mode == StatePersistMode.Read)
            {
                _drawables.Clear();
                _drawables.Capacity = drawablesCount;

                for (var i = 0; i < drawablesCount; i++)
                {
                    reader.BeginObject();

                    ushort objectDefinitionId = 0;
                    reader.PersistUInt16("ObjectDefinitionId", ref objectDefinitionId);
                    var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                    reader.BeginSegment(objectDefinition.Name);

                    var objectID = 0u;
                    reader.PersistUInt32("ObjectId", ref objectID);

                    var gameObject = _gameLogic.GetObjectById(objectID);

                    reader.PersistObject("Drawable", gameObject.Drawable);

                    _drawables.Add(gameObject.Drawable);
                    _drawablesById[gameObject.Drawable.DrawableID] = gameObject.Drawable;

                    reader.EndSegment();

                    reader.EndObject();
                }
            }
            else
            {
                foreach (var drawable in _drawables)
                {
                    reader.BeginObject();

                    var gameObject = drawable.GameObject;

                    var objectDefinitionId = _objectDefinitionLookupTable.GetId(gameObject.Definition);
                    reader.PersistUInt16("ObjectDefinitionId", ref objectDefinitionId);

                    reader.BeginSegment(gameObject.Definition.Name);

                    var objectID = gameObject.ID;
                    reader.PersistUInt32("ObjectId", ref objectID);

                    reader.PersistObject("Drawable", gameObject.Drawable);

                    reader.EndSegment();

                    reader.EndObject();
                }
            }
            reader.EndArray();

            reader.PersistListWithUInt32Count("BriefingTexts", _briefingTexts, static (StatePersister persister, ref string item) =>
            {
                persister.PersistAsciiStringValue(ref item);
            });
        }
    }
}

using System.Collections.Generic;
using OpenSage.Logic;
using OpenSage.Logic.Object;

namespace OpenSage.Client
{
    internal sealed class GameClient : DisposableBase, IPersistableObject
    {
        private readonly Game _game;
        private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;
        private readonly List<Drawable> _drawables = new();
        private readonly List<string> _briefingTexts = new();

        private uint _currentFrame;

        internal uint NextDrawableId;

        public GameClient(Game game)
        {
            _game = game;
            _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(game.AssetStore.ObjectDefinitions);
        }

        public Drawable CreateDrawable(ObjectDefinition objectDefinition, GameObject gameObject)
        {
            var drawable = AddDisposable(new Drawable(objectDefinition, _game.Scene3D.GameContext, gameObject));

            drawable.ID = NextDrawableId++;

            return drawable;
        }

        internal void OnDrawableIdChanged(Drawable drawable, uint oldDrawableId)
        {
            if (oldDrawableId != 0)
            {
                SetDrawable(oldDrawableId, null);
            }

            SetDrawable(drawable.ID, drawable);
        }

        private void SetDrawable(uint drawableId, Drawable drawable)
        {
            while (_drawables.Count <= drawableId)
            {
                _drawables.Add(null);
            }
            _drawables[(int)drawableId] = drawable;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(3);

            reader.PersistUInt32(ref _currentFrame);
            reader.PersistObject(_objectDefinitionLookupTable, "ObjectDefinitions");

            var drawablesCount = (ushort)_drawables.Count;
            reader.PersistUInt16(ref drawablesCount);

            reader.BeginArray("Drawables");
            if (reader.Mode == StatePersistMode.Read)
            {
                _drawables.Clear();
                _drawables.Capacity = drawablesCount;

                for (var i = 0; i < drawablesCount; i++)
                {
                    reader.BeginObject();

                    ushort objectDefinitionId = 0;
                    reader.PersistUInt16(ref objectDefinitionId);
                    var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                    reader.BeginSegment(objectDefinition.Name);

                    var objectID = 0u;
                    reader.PersistUInt32(ref objectID);

                    var drawable = objectID != 0u
                        ? _game.GameLogic.GetObjectById(objectID).Drawable
                        : CreateDrawable(objectDefinition, null);

                    reader.PersistObject(drawable);

                    reader.EndSegment();

                    reader.EndObject();
                }
            }
            else
            {
                foreach (var drawable in _drawables)
                {
                    if (drawable == null)
                    {
                        continue;
                    }

                    reader.BeginObject();

                    var objectDefinitionId = _objectDefinitionLookupTable.GetId(drawable.Definition);
                    reader.PersistUInt16(ref objectDefinitionId);

                    reader.BeginSegment(drawable.Definition.Name);

                    var objectID = drawable.GameObjectID;
                    reader.PersistUInt32(ref objectID);

                    reader.PersistObject(drawable);

                    reader.EndSegment();

                    reader.EndObject();
                }
            }
            reader.EndArray();

            reader.PersistListWithUInt32Count(
                _briefingTexts,
                static (StatePersister persister, ref string item) =>
                {
                    persister.PersistAsciiStringValue(ref item);
                },
                "BriefingTexts");
        }
    }
}

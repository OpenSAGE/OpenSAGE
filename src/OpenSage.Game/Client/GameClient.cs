using System.Collections.Generic;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Client;

internal sealed class GameClient : DisposableBase, IPersistableObject
{
    private readonly IGame _game;
    private readonly ObjectDefinitionLookupTable _objectDefinitionLookupTable;
    private readonly List<Drawable> _drawables = new();
    private readonly List<string> _briefingTexts = new();

    private uint _currentFrame;

    internal uint NextDrawableId = 1;

    public readonly IRandom Random;

    public GameClient(IGame game)
    {
        _game = game;
        _objectDefinitionLookupTable = new ObjectDefinitionLookupTable(game.AssetStore.ObjectDefinitions);

        Random = game.CreateRandom();
    }

    public Drawable CreateDrawable(ObjectDefinition objectDefinition, GameObject gameObject)
    {
        var drawable = AddDisposable(new Drawable(objectDefinition, _game.Scene3D.GameEngine, gameObject));

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

    public void DestroyDrawable(Drawable drawable)
    {
        drawable.Dispose();

        RemoveToDispose(drawable);

        _drawables[(int)drawable.ID] = null;
    }

    private void DestroyAllDrawablesNow()
    {
        foreach (var drawable in _drawables)
        {
            if (drawable != null)
            {
                drawable.Dispose();

                RemoveToDispose(drawable);
            }
        }

        _drawables.Clear();
    }

    public void Reset()
    {
        DestroyAllDrawablesNow();

        NextDrawableId = 1;
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
            DestroyAllDrawablesNow();
            _drawables.EnsureCapacity(drawablesCount);

            for (var i = 0; i < drawablesCount; i++)
            {
                reader.BeginObject();

                ushort objectDefinitionId = 0;
                reader.PersistUInt16(ref objectDefinitionId);
                var objectDefinition = _objectDefinitionLookupTable.GetById(objectDefinitionId);

                reader.BeginSegment(objectDefinition.Name);

                var objectId = ObjectId.Invalid;
                reader.PersistObjectId(ref objectId);

                var drawable = objectId.IsValid
                    ? _game.GameLogic.GetObjectById(objectId).Drawable
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

                var objectId = drawable.GameObjectID;
                reader.PersistObjectId(ref objectId);

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

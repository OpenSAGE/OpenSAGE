using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Client;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators;

// TODO: Cancel this when:
// 1. Structure dies
// 2. We lose access to the building
public sealed class RallyPointOrderGenerator : OrderGenerator, IDisposable
{
    public override bool CanDrag => true;

    private readonly GameObject _gameObject;
    private readonly Drawable _rallyPointMarker;

    private OrderType _currentOrder = OrderType.Zero;

    public RallyPointOrderGenerator(Game game, GameObject gameObject)
        : base(game)
    {
        _gameObject = gameObject;

        var rpMarkerDef = Game.AssetStore.ObjectDefinitions.GetByName("RallyPointMarker");
        _rallyPointMarker = Game.GameClient.CreateDrawable(rpMarkerDef, null);
    }

    public override OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
    {
        var playerId = scene.GetPlayerIndex(LocalPlayer);

        if (_currentOrder is OrderType.SetSelection)
        {
            if (WorldObject == null)
            {
                throw new InvalidStateException("World object null for set selection order");
            }

            var setSelectionOrder = Order.CreateSetSelection(playerId, WorldObject.ID);

            return OrderGeneratorResult.SuccessAndContinue(new[] { setSelectionOrder });
        }

        if (SelectedUnits == null)
        {
            throw new InvalidStateException("Local player not present when setting rally point");
        }

        var objectIds = new List<ObjectId>();
        foreach (var gameObject in SelectedUnits)
        {
            objectIds.Add(gameObject.ID);
        }

        var order = Order.CreateSetRallyPointOrder(playerId, objectIds, WorldPosition);

        return OrderGeneratorResult.SuccessAndContinue(new[] { order });
    }

    public override string GetCursor(KeyModifiers keyModifiers)
    {
        _currentOrder = GetCurrentOrder();
        return Cursors.CursorForOrder(_currentOrder);
    }

    private OrderType GetCurrentOrder()
    {
        return WorldObject != null ? OrderType.SetSelection : OrderType.SetRallyPoint;
    }

    public override void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
    {
        if (_gameObject.RallyPoint == null)
        {
            return;
        }

        _rallyPointMarker.Transform.Translation = _gameObject.RallyPoint.Value;

        var renderItemConstantsPS = new MeshShaderResources.RenderItemConstantsPS
        {
            Opacity = 1.0f,
            TintColor = Vector3.One,
        };

        _rallyPointMarker.BuildRenderList(
            renderList,
            camera,
            gameTime,
            _rallyPointMarker.TransformMatrix,
            renderItemConstantsPS);
    }

    public void Dispose()
    {
        Game.GameClient.DestroyDrawable(_rallyPointMarker);
    }
}

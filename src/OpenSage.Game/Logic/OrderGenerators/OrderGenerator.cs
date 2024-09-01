#nullable enable

using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.OrderGenerators;

public abstract class OrderGenerator(IGame game) : IOrderGenerator
{
    public abstract bool CanDrag { get; }

    protected Vector3 WorldPosition { get; private set; }
    protected GameObject? WorldObject { get; private set; }
    protected IGame Game => game;
    protected Player? LocalPlayer => game.Scene3D.LocalPlayer;
    protected IReadOnlyCollection<GameObject>? SelectedUnits => LocalPlayer?.SelectedUnits;

    public abstract OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers);
    public abstract string? GetCursor(KeyModifiers keyModifiers);

    public virtual void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime) { }
    public virtual void UpdateDrag(Vector3 position) { }

    public virtual void UpdatePosition(Vector2 mousePosition, Vector3 worldPosition)
    {
        WorldPosition = worldPosition;
        WorldObject = game.Selection.FindClosestObject(mousePosition);
    }
}

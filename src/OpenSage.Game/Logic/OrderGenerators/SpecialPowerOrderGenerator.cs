using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Logic.OrderGenerators
{
    public enum SpecialPowerTarget
    {
        Location,
        AllyObject,
        EnemyObject,
        NeutralObject,
    };

    public sealed class SpecialPowerOrderGenerator : IOrderGenerator, IDisposable
    {
        private readonly SpecialPower _specialPower;
        private readonly GameData _config;
        private readonly SpecialPowerTarget _target;
        private readonly Scene3D _scene;

        private readonly DecalHandle _decalHandle;

        private Vector3 _position;

        public bool CanDrag => false;

        internal SpecialPowerOrderGenerator(
           SpecialPower specialPower,
           GameData config,
           Player player,
           GameContext gameContext,
           SpecialPowerTarget target,
           Scene3D scene,
           in TimeInterval time)
        {
            _specialPower = specialPower;
            _target = target;
            _scene = scene;
            _config = config;

            // TODO: Improve this check.
            var radiusCursors = _scene.GameContext.AssetLoadContext.AssetStore.InGameUI.Current.RadiusCursors;
            var radiusCursorName = _specialPower.Type.ToString();
            if (radiusCursors.TryGetValue(radiusCursorName, out var radiusCursor))
            {
                _decalHandle = scene.Terrain.RadiusCursorDecals.AddDecal(
                    radiusCursor.DecalTemplate,
                    _specialPower.RadiusCursorRadius,
                    time);
            }
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime) { }

        public OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
        {
            var player = scene.LocalPlayer;
            var playerIdx = scene.GetPlayerIndex(player);
            Order specialPowerOrder;
            if (_target == SpecialPowerTarget.Location)
            {
                specialPowerOrder = Order.CreateSpecialPowerAtLocation(playerIdx, _specialPower.InternalId, _position);
            }
            else
            {
                specialPowerOrder = Order.CreateSpecialPowerAtObject(playerIdx, _specialPower.InternalId);
            }
            return OrderGeneratorResult.SuccessAndExit(new[] { specialPowerOrder });
        }

        public void UpdateDrag(Vector3 position)
        {
        }

        public void UpdatePosition(Vector2 mousePosition, Vector3 worldPosition)
        {
            _position = worldPosition;

            if (_decalHandle != null)
            {
                _scene.Terrain.RadiusCursorDecals.SetDecalPosition(_decalHandle, worldPosition.Vector2XY());
            }
        }

        public string GetCursor(KeyModifiers keyModifiers) => "Target";

        public void Dispose()
        {
            if (_decalHandle != null)
            {
                _scene.Terrain.RadiusCursorDecals.RemoveDecal(_decalHandle);
            }
        }
    }
}

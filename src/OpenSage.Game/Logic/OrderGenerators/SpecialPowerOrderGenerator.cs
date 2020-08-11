using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;

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

        private float _angle;
        private Vector3 _position;

        public bool CanDrag => false;

        internal SpecialPowerOrderGenerator(
           SpecialPower specialPower,
           GameData config,
           Player player,
           GameContext gameContext,
           SpecialPowerTarget target,
           Scene3D scene)
        {
            _specialPower = specialPower;
            _target = target;
            _scene = scene;
            _config = config;
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            // TODO:
            // throw new NotImplementedException();
        }

        public void Dispose()
        {
            // TODO:
            // throw new NotImplementedException();
        }

        public OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
        {
            var player = scene.LocalPlayer;
            var playerIdx = scene.GetPlayerIndex(player);
            Order specialPowerOrder = null;
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
        }

        // Use radial cursor.
        public string GetCursor(KeyModifiers keyModifiers) => null;
    }
}

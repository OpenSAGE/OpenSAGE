using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators
{
    public sealed class SpecialPowerOrderGenerator : IOrderGenerator, IDisposable
    {
        private readonly SpecialPower _specialPower;
        private readonly int _definitionIndex;
        private readonly GameData _config;
        private readonly Scene3D _scene;

        private float _angle;
        private Vector3 _position;

        public bool CanDrag => false;

        internal SpecialPowerOrderGenerator(
           SpecialPower specialPower,
           GameData config,
           Player player,
           GameContext gameContext,
           Scene3D scene)
        {
            _specialPower = specialPower;
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

        public OrderGeneratorResult TryActivate(Scene3D scene)
        {
            var player = scene.LocalPlayer;
            var playerIdx = scene.GetPlayerIndex(player);
            var specialPowerOrder = Order.CreateSpecialPowerAtLocation(playerIdx, _specialPower.InternalId, _position);

            return OrderGeneratorResult.SuccessAndExit(new[] { specialPowerOrder });
        }

        public void UpdateDrag(Vector3 position)
        {
        }

        public void UpdatePosition(Vector3 position)
        {
            _position = position;
        }
    }
}

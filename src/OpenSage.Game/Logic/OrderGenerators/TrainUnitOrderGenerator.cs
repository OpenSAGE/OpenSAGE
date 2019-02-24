using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.OrderGenerators
{
    // TODO: Cancel this when:
    // 1. Building gets destroyed
    // 2. We lose access to the unit
    public sealed class TrainUnitOrderGenerator : IOrderGenerator
    {
        private readonly ObjectDefinition _unitDefinition;
        private readonly int _definitionIndex;
        private readonly GameData _config;
        private readonly float _baseAngle;

        private float _angle;
        private Vector3 _position;

        public bool CanDrag { get; } = true;

        public TrainUnitOrderGenerator(ObjectDefinition buildingDefinition, int definitionIndex, GameData config)
        {
            _unitDefinition = buildingDefinition;
            _definitionIndex = definitionIndex;
            _config = config;

            // TODO: Should this be relative to the current camera angle?
            _baseAngle = MathUtility.ToRadians(_unitDefinition.PlacementViewAngle);
            _angle = _baseAngle;
        }

        public void BuildRenderList(RenderList renderList)
        {
        }

        public OrderGeneratorResult TryActivate(Scene3D scene)
        {
            var transform = new Transform(_position, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _angle));

            var order = Order.CreateBuildObject(scene.GetPlayerIndex(scene.LocalPlayer), _definitionIndex, _position, _angle);

            // TODO: Check that we still have enough money

            return OrderGeneratorResult.SuccessAndExit(new[] { order });
        }

        public void UpdatePosition(Vector3 position)
        {
            _position = position;
        }

        public void UpdateDrag(Vector2 mouseDelta)
        {
        }
    }
}

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
    // 1. Builder dies
    // 2. We lose access to the building
    public sealed class ConstructBuildingOrderGenerator : IOrderGenerator
    {
        private readonly ObjectDefinition _buildingDefinition;
        private readonly int _definitionIndex;
        private readonly GameData _config;
        private readonly float _baseAngle;

        private float _angle;
        private Vector3 _position;

        public bool CanDrag { get; } = true;

        public ConstructBuildingOrderGenerator(ObjectDefinition buildingDefinition, int definitionIndex, GameData config)
        {
            _buildingDefinition = buildingDefinition;
            _definitionIndex = definitionIndex;
            _config = config;

            // TODO: Should this be relative to the current camera angle?
            _baseAngle = MathUtility.ToRadians(_buildingDefinition.PlacementViewAngle);
            _angle = _baseAngle;
        }

        public void BuildRenderList(RenderList renderList)
        {
            // TODO draw building ghost
        }

        public OrderGeneratorResult TryActivate(Scene3D scene)
        {
            var transform = new Transform(_position, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _angle));

            // TODO: The collider should be created earlier and updated in UpdatePosition/UpdateDrag
            // TODO: This should work for all collider types.
            if (Collider.Create(_buildingDefinition, transform) is BoxCollider collider)
            {
                // TODO: Optimise using a quadtree
                foreach (var obj in scene.GameObjects.Items)
                {
                    if (!(obj.Collider is BoxCollider otherCollider))
                    {
                        continue;
                    }

                    if (collider.Intersects(otherCollider))
                    {
                        return OrderGeneratorResult.Failure("Intersects an another building.");
                    }
                }
            }

            var player = scene.LocalPlayer;
            if (player.Money < _buildingDefinition.BuildCost)
            {
                return OrderGeneratorResult.Failure("Not enough cash");
            }
            else
            {
                player.Money -= (uint) _buildingDefinition.BuildCost;
            }

            var playerIdx = scene.GetPlayerIndex(scene.LocalPlayer);
            var moveOrder = Order.CreateMoveOrder(playerIdx, _position);
            var buildOrder = Order.CreateBuildObject(playerIdx, _definitionIndex, _position, _angle);

            // TODO: Check that the target area has been explored
            // TODO: Check that the builder can reach target position
            // TODO: Check that the terrain is even enough at the target position

            // TODO: Also send an order to builder to start building.
            return OrderGeneratorResult.SuccessAndExit(new[] { moveOrder, buildOrder });
        }

        public void UpdatePosition(Vector3 position)
        {
            _position = position;
        }

        public void UpdateDrag(Vector2 mouseDelta)
        {
            // TODO: Make this configurable and figure out the real values from SAGE.
            const int pixelsPerRotation = 250;
            _angle = _baseAngle + mouseDelta.X / pixelsPerRotation * (float) (2 * Math.PI);
        }
    }
}

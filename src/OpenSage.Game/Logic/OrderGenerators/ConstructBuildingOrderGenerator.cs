using System;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.OrderGenerators
{
    // TODO: Cancel this when:
    // 1. Builder dies
    // 2. We lose access to the building
    // 3. Player right-clicks
    public sealed class ConstructBuildingOrderGenerator : IOrderGenerator, IDisposable
    {
        private readonly ObjectDefinition _buildingDefinition;
        private readonly int _definitionIndex;
        private readonly GameData _config;
        private readonly float _baseAngle;

        private readonly GameObject _previewObject;

        private readonly BoxCollider _collider;

        private float _angle;
        private Vector3 _position;

        public bool CanDrag { get; } = true;

        internal ConstructBuildingOrderGenerator(ObjectDefinition buildingDefinition, int definitionIndex, GameData config, Player player, AssetLoadContext loadContext)
        {
            _buildingDefinition = buildingDefinition;
            _definitionIndex = definitionIndex;
            _config = config;

            // TODO: Should this be relative to the current camera angle?
            _baseAngle = MathUtility.ToRadians(_buildingDefinition.PlacementViewAngle);
            _angle = _baseAngle;

            _previewObject = new GameObject(buildingDefinition, loadContext, player, null, null);
            _previewObject.IsPlacementPreview = true;

            UpdatePreviewObjectPosition();
            UpdatePreviewObjectAngle();

            // TODO: This should work for all collider types.
            _collider = Collider.Create(_buildingDefinition, _previewObject.Transform) as BoxCollider;
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            _previewObject.LocalLogicTick(gameTime, 0, null);
            _previewObject.BuildRenderList(renderList, camera);
        }

        public OrderGeneratorResult TryActivate(Scene3D scene)
        {
            if (_collider != null)
            {
                // TODO: Optimise using a quadtree
                foreach (var obj in scene.GameObjects.Items)
                {
                    if (!(obj.Collider is BoxCollider otherCollider))
                    {
                        continue;
                    }

                    if (_collider.Intersects(otherCollider))
                    {
                        return OrderGeneratorResult.Failure("Intersects an another building.");
                    }
                }
            }

            var player = scene.LocalPlayer;
            if (player.Money < _buildingDefinition.BuildCost)
            {
                return OrderGeneratorResult.Failure("Not enough cash for construction");
            }

            var playerIdx = scene.GetPlayerIndex(player);
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

            UpdatePreviewObjectPosition();
        }

        private void UpdatePreviewObjectPosition()
        {
            _previewObject.Transform.Translation = _position;
        }

        public void UpdateDrag(Vector2 mouseDelta)
        {
            // TODO: Make this configurable and figure out the real values from SAGE.
            const int pixelsPerRotation = 250;
            _angle = _baseAngle + mouseDelta.X / pixelsPerRotation * (float) (2 * Math.PI);

            UpdatePreviewObjectAngle();
        }

        private void UpdatePreviewObjectAngle()
        {
            _previewObject.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _angle);
        }

        public void Dispose()
        {
            _previewObject.Dispose();
        }
    }
}

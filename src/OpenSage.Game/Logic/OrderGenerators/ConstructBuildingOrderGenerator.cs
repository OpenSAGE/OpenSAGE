using System;
using System.Linq;
using System.Numerics;
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
        private readonly Scene3D _scene;
        private readonly float _baseAngle;

        private readonly GameObject _previewObject;

        private readonly BoxCollider _collider;

        private float _angle;
        private Vector3 _position;

        public bool CanDrag { get; } = true;

        internal ConstructBuildingOrderGenerator(
            ObjectDefinition buildingDefinition,
            int definitionIndex,
            GameData config,
            Player player,
            GameContext gameContext,
            Scene3D scene)
        {
            _buildingDefinition = buildingDefinition;
            _definitionIndex = definitionIndex;
            _config = config;
            _scene = scene;

            // TODO: Should this be relative to the current camera angle?
            _baseAngle = MathUtility.ToRadians(_buildingDefinition.PlacementViewAngle);
            _angle = _baseAngle;

            _previewObject = new GameObject(
                buildingDefinition,
                gameContext,
                player,
                null)
            {
                IsPlacementPreview = true
            };

            UpdatePreviewObjectPosition();
            UpdatePreviewObjectAngle();
            UpdateValidity();

            // TODO: This should work for all collider types.
            _collider = Collider.Create(_buildingDefinition, _previewObject.Transform) as BoxCollider;
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            // TODO: Draw arrow (locater02.w3d) to visualise rotation angle.

            _previewObject.LocalLogicTick(gameTime, 0, null);
            _previewObject.BuildRenderList(renderList, camera, gameTime);
        }

        public OrderGeneratorResult TryActivate(Scene3D scene)
        {
            // TODO: Probably not right way to get dozer object.
            var dozer = scene.LocalPlayer.SelectedUnits.First();

            if (!IsValidPosition())
            {
                scene.Audio.PlayAudioEvent(dozer.Definition.UnitSpecificSounds?.Assets["VoiceNoBuild"]?.Value);

                // TODO: Display correct message:
                // - GUI:CantBuildRestrictedTerrain
                // - GUI:CantBuildNotFlatEnough
                // - GUI:CantBuildObjectsInTheWay
                // - GUI:CantBuildNoClearPath
                // - GUI:CantBuildShroud
                // - GUI:CantBuildThere

                return OrderGeneratorResult.Failure("Invalid position.");
            }

            var player = scene.LocalPlayer;
            if (player.Money < _buildingDefinition.BuildCost)
            {
                return OrderGeneratorResult.Failure("Not enough cash for construction");
            }

            scene.Audio.PlayAudioEvent(dozer.Definition.UnitSpecificSounds?.Assets["VoiceCreate"]?.Value);

            var playerIdx = scene.GetPlayerIndex(player);
            var moveOrder = Order.CreateMoveOrder(playerIdx, _position);
            var buildOrder = Order.CreateBuildObject(playerIdx, _definitionIndex, _position, _angle);

            // TODO: Also send an order to builder to start building.
            return OrderGeneratorResult.SuccessAndExit(new[] { moveOrder, buildOrder });
        }

        private bool IsValidPosition()
        {
            // TODO: Check that the target area has been explored
            // TODO: Check that the builder can reach target position
            // TODO: Check that the terrain is even enough at the target position

            if (_collider != null)
            {
                // TODO: Optimise using a quadtree
                foreach (var obj in _scene.GameObjects.Items)
                {
                    if (!(obj.Collider is BoxCollider otherCollider))
                    {
                        continue;
                    }

                    // TODO: Should use FactoryExitWidth
                    if (_collider.Intersects(otherCollider))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void UpdatePosition(Vector3 position)
        {
            _position = position;

            UpdatePreviewObjectPosition();
            UpdateValidity();
        }

        private void UpdatePreviewObjectPosition()
        {
            _previewObject.Transform.Translation = _position;
        }

        public void UpdateDrag(Vector3 position)
        {
            // Calculate angle from building position to current unprojected mouse position.
            var direction = position.Vector2XY() - _position.Vector2XY();
            _angle = MathUtility.GetYawFromDirection(direction);

            UpdatePreviewObjectAngle();
            UpdateValidity();
        }

        private void UpdatePreviewObjectAngle()
        {
            _previewObject.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _angle);
        }

        private void UpdateValidity()
        {
            _previewObject.IsPlacementInvalid = !IsValidPosition();
        }

        public void Dispose()
        {
            _previewObject.Dispose();
        }
    }
}

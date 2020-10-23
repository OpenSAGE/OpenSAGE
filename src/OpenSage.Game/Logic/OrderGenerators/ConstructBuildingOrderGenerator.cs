using System;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
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

        private readonly GameObject _previewObject;

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
            _angle = MathUtility.ToRadians(_buildingDefinition.PlacementViewAngle);

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
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            // TODO: Draw arrow (locater02.w3d) to visualise rotation angle.

            _previewObject.LocalLogicTick(gameTime, 0, null);
            _previewObject.BuildRenderList(renderList, camera, gameTime);
        }

        public OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
        {
            if (scene.Game.SageGame == SageGame.Bfme)
            {
                return OrderGeneratorResult.Inapplicable();
            }

            // TODO: Probably not right way to get dozer object.
            var dozer = scene.LocalPlayer.SelectedUnits.First();

            if (!IsValidPosition())
            {
                scene.Audio.PlayAudioEvent(dozer, dozer.Definition.UnitSpecificSounds?["VoiceNoBuild"]?.Value);

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

            scene.Audio.PlayAudioEvent(dozer, dozer.Definition.UnitSpecificSounds?["VoiceCreate"]?.Value);

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

            if (_previewObject.Collider == null)
            {
                return true;
            }

            _scene.Quadtree.Remove(_previewObject);
            return !_scene.Quadtree.FindIntersecting(_previewObject.Collider).Any();
        }

        public void UpdatePosition(Vector2 mousePosition, Vector3 worldPosition)
        {
            _position = worldPosition;

            UpdatePreviewObjectPosition();
            UpdateValidity();
        }

        private void UpdatePreviewObjectPosition()
        {
            _previewObject.SetTranslation(_position);
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
            _previewObject.SetRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _angle));
        }

        private void UpdateValidity()
        {
            // TODO: draw collider bounds in debug view when 'showBounds'
            _previewObject.IsPlacementInvalid = !IsValidPosition();
        }

        // Use radial cursor.
        public string GetCursor(KeyModifiers keyModifiers) => null;

        public void Dispose()
        {
            _previewObject.Dispose();
        }
    }
}

﻿using System;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Logic.OrderGenerators;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic
{
    public class OrderGeneratorSystem : GameSystem
    {
        private IOrderGenerator _activeGenerator;

        public IOrderGenerator ActiveGenerator
        {
            get => _activeGenerator;
            set
            {
                if (_activeGenerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _activeGenerator = value;
            }
        }

        public bool HasActiveOrderGenerator => ActiveGenerator != null;

        public OrderGeneratorSystem(Game game) : base(game) { }

        private Vector3? _worldPosition;

        public void UpdatePosition(Vector2 mousePosition)
        {
            _worldPosition = GetTerrainPosition(mousePosition);

            if (_worldPosition.HasValue && ActiveGenerator != null)
            {
                ActiveGenerator.UpdatePosition(_worldPosition.Value);
            }
        }

        public void UpdateDrag(Vector2 mousePosition)
        {
            var worldPosition = GetTerrainPosition(mousePosition);

            if (worldPosition.HasValue && ActiveGenerator != null)
            {
                ActiveGenerator.UpdateDrag(worldPosition.Value);
            }
        }

        private bool StructuresSelected()
        {
            if (Game.Scene3D.LocalPlayer.SelectedUnits.Count == 0)
            {
                return false;
            }

            bool result = true;

            foreach (var unit in Game.Scene3D.LocalPlayer.SelectedUnits)
            {
                if (!unit.Definition.KindOf.Get(ObjectKinds.Structure))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public void OnRightClick(bool ctrlDown)
        {
            if (!_worldPosition.HasValue)
            {
                return;
            }

            if (Game.Scene3D.LocalPlayer.SelectedUnits.Count == 0)
            {
                return;
            }

            var canSetRallyPoint = false;
            foreach (var unit in Game.Scene3D.LocalPlayer.SelectedUnits)
            {
                if (unit.Definition.KindOf.Get(ObjectKinds.AutoRallyPoint))
                {
                    canSetRallyPoint = true;
                    break;
                }
            }

            Order order = null;

            if (canSetRallyPoint)
            {
                var playerId = Game.Scene3D.GetPlayerIndex(Game.Scene3D.LocalPlayer);
                var objectIds = Game.Scene3D.GameObjects.GetObjectIds(Game.Scene3D.LocalPlayer.SelectedUnits);
                order = Order.CreateSetRallyPointOrder(playerId, objectIds, _worldPosition.Value);
            }
            else
            {
                // TODO: Use ini files for this, don't hardcode it.
                if (ctrlDown)
                {
                    // TODO: Check whether clicked point is an object, or empty ground.
                    //var unit = Game.Scene3D.LocalPlayer.SelectedUnits.Last();
                    //unit.OnLocalMove(Game.Audio);

                    order = Order.CreateAttackGround(Game.Scene3D.GetPlayerIndex(Game.Scene3D.LocalPlayer), _worldPosition.Value);
                }
                else
                {
                    // TODO: Check whether at least one of the selected units can actually be moved.

                    // We choose the sound based on the most-recently-selected unit.
                    var unit = Game.Scene3D.LocalPlayer.SelectedUnits.Last();
                    unit.OnLocalMove(Game.Audio);

                    order = Order.CreateMoveOrder(Game.Scene3D.GetPlayerIndex(Game.Scene3D.LocalPlayer), _worldPosition.Value);
                }
            }

            if (order != null)
            {
                Game.NetworkMessageBuffer.AddLocalOrder(order);
            }
        }

        public void OnActivate()
        {
            if (!_worldPosition.HasValue || ActiveGenerator == null)
            {
                return;
            }

            var result = ActiveGenerator.TryActivate(Game.Scene3D);

            switch (result)
            {
                case OrderGeneratorResult.Success success:
                    {
                        foreach (var order in success.Orders)
                        {
                            Game.NetworkMessageBuffer.AddLocalOrder(order);
                        }

                        if (success.Exit)
                        {
                            ActiveGenerator = null;
                        }

                        break;
                    }
                case OrderGeneratorResult.FailureResult _:
                    // TODO: Show error message in HUD
                    break;
            }
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            ActiveGenerator?.BuildRenderList(renderList, camera, gameTime);
        }

        private Vector3? GetTerrainPosition(Vector2 mousePosition)
        {
            var ray = Game.Scene3D.Camera.ScreenPointToRay(mousePosition);
            return Game.Scene3D.Terrain.Intersect(ray);
        }

        public void StartConstructBuilding(ObjectDefinition buildingDefinition)
        {
            if (!buildingDefinition.KindOf.Get(ObjectKinds.Structure))
            {
                throw new ArgumentException("Building must have the STRUCTURE kind.", nameof(buildingDefinition));
            }

            // TODO: Handle ONLY_BY_AI
            // TODO: Copy default settings from DefaultThingTemplate
            /*if (buildingDefinition.Buildable != ObjectBuildableType.Yes)
            {
                throw new ArgumentException("Building must be buildable.", nameof(buildingDefinition));
            }*/

            // TODO: Check that the builder can build that building.
            // TODO: Check that the building has been unlocked.
            // TODO: Check that the builder isn't building something else right now?

            var gameData = Game.AssetStore.GameData.Current;
            var definitionIndex = buildingDefinition.InternalId;

            ActiveGenerator = new ConstructBuildingOrderGenerator(buildingDefinition, definitionIndex, gameData, Game.Scene3D.LocalPlayer, Game.Scene3D.GameContext, Game.Scene3D);
        }

        public void StartConstructUnit(ObjectDefinition unitDefinition)
        {
            // TODO: Handle ONLY_BY_AI
            // TODO: Copy default settings from DefaultThingTemplate

            // TODO: Check that the building can build that unit.
            // TODO: Check that the unit has been unlocked.
            // TODO: Check that the building isn't building something else right now?

            var gameData = Game.AssetStore.GameData.Current;
            var definitionIndex = unitDefinition.InternalId;

            ActiveGenerator = new TrainUnitOrderGenerator(unitDefinition, definitionIndex, gameData);
        }

        public void SetRallyPoint()
        {
            ActiveGenerator = new RallyPointOrderGenerator();
        }
    }
}

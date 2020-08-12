using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.OrderGenerators;

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
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (_activeGenerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _activeGenerator = value;
            }
        }

        public OrderGeneratorSystem(Game game)
            : base(game)
        {
            _activeGenerator = new UnitOrderGenerator(game);
        }

        private Vector3? _worldPosition;

        public void UpdatePosition(Vector2 mousePosition)
        {
            _worldPosition = GetTerrainPosition(mousePosition);

            if (_worldPosition.HasValue && ActiveGenerator != null)
            {
                ActiveGenerator.UpdatePosition(mousePosition, _worldPosition.Value);
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

        public bool TryActivate(KeyModifiers keyModifiers)
        {
            if (!_worldPosition.HasValue)
            {
                return false;
            }

            var result = ActiveGenerator.TryActivate(Game.Scene3D, keyModifiers);

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
                            ActiveGenerator = new UnitOrderGenerator(Game);
                        }

                        return true;
                    }

                case OrderGeneratorResult.InapplicableResult _:
                    return false;

                case OrderGeneratorResult.FailureResult _:
                    // TODO: Show error message in HUD
                    return true;

                default:
                    throw new InvalidOperationException();
            }
        }

        public void Update(in TimeInterval time, KeyModifiers keyModifiers)
        {
            var cursor = ActiveGenerator.GetCursor(keyModifiers);
            if (cursor != null)
            {
                Game.Window.IsMouseVisible = true;
                Game.Cursors.SetCursor(cursor, time);
            }
            else
            {
                Game.Window.IsMouseVisible = false;
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

        public void StartSpecialPowerAtLocation(SpecialPower specialPower)
        {
            var gameData = Game.AssetStore.GameData.Current;

            ActiveGenerator = new SpecialPowerOrderGenerator(specialPower, gameData, Game.Scene3D.LocalPlayer,
                    Game.Scene3D.GameContext, SpecialPowerTarget.Location, Game.Scene3D, Game.MapTime);
        }

        public void StartSpecialPowerAtObject(SpecialPower specialPower)
        {
            var gameData = Game.AssetStore.GameData.Current;

            //TODO: pass the right target type
            ActiveGenerator = new SpecialPowerOrderGenerator(specialPower, gameData, Game.Scene3D.LocalPlayer,
                    Game.Scene3D.GameContext, SpecialPowerTarget.EnemyObject, Game.Scene3D, Game.MapTime);
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

        public void SetRallyPoint()
        {
            ActiveGenerator = new RallyPointOrderGenerator();
        }

        public void CancelOrderGenerator()
        {
            ActiveGenerator = new UnitOrderGenerator(Game);
        }
    }
}

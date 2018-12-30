using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Logic.OrderGenerators;

namespace OpenSage.Logic
{
    public class OrderGeneratorSystem : GameSystem
    {
        public IOrderGenerator ActiveGenerator;

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

        public void OnActivate()
        {
            if (!_worldPosition.HasValue || ActiveGenerator == null)
            {
                return;
            }

            var result = ActiveGenerator.TryActivate(Game.Scene3D);

            if (result is OrderGeneratorResult.Success success)
            {
                // TODO: Wrong place, wrong behavior.
                Game.Audio.PlayAudioEvent("DozerUSAVoiceBuild");

                foreach (var order in success.Orders)
                {
                    Game.NetworkMessageBuffer.AddLocalOrder(order);
                }

                if (success.Exit)
                {
                    ActiveGenerator = null;
                }
            }
            else if (result is OrderGeneratorResult.FailureResult failure)
            {
                // TODO: Wrong place, wrong behavior.
                Game.Audio.PlayAudioEvent("DozerUSAVoiceBuildNot");
                // TODO: Show error message in HUD
            }
        }

        private Vector3? GetTerrainPosition(Vector2 mousePosition)
        {
            var ray = Game.Scene3D.Camera.ScreenPointToRay(mousePosition);
            return Game.Scene3D.Terrain.Intersect(ray);
        }

        public void StartConstructBuilding(ObjectDefinition buildingDefinition, GameObject builder)
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

            var gameData = Game.ContentManager.IniDataContext.GameData;
            var definitionIndex = Game.ContentManager.IniDataContext.Objects.IndexOf(buildingDefinition) + 1;

            ActiveGenerator = new ConstructBuildingOrderGenerator(buildingDefinition, definitionIndex, builder, gameData);
        }
    }
}

using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Logic.OrderGenerators;

namespace OpenSage.Logic
{
    public class OrderGeneratorSystem : GameSystem
    {
        private IOrderGenerator _currentOrderGenerator;

        public OrderGeneratorSystem(Game game) : base(game) { }

        public bool OnClick(Vector2 mousePosition)
        {
            if (_currentOrderGenerator == null)
            {
                return false;
            }

            var ray = Game.Scene3D.Camera.ScreenPointToRay(mousePosition);
            var position = Game.Scene3D.Terrain.Intersect(ray);

            if (position.HasValue)
            {
                var result = _currentOrderGenerator.OnActivate(Game.Scene3D, position.Value);

                if (result is OrderGeneratorResult.Success success)
                {
                    foreach (var order in success.Orders)
                    {
                        Game.NetworkMessageBuffer.AddLocalOrder(order);
                    }

                    if (success.Exit)
                    {
                        _currentOrderGenerator = null;
                    }
                }
                else if (result is OrderGeneratorResult.FailureResult failure)
                {
                    // TODO: Show error message in HUD and play EVA event.
                }
            }

            return true;
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

            _currentOrderGenerator = new ConstructBuildingOrderGenerator(buildingDefinition, definitionIndex, builder, gameData);
        }
    }
}

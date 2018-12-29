using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators
{
    // TODO: Cancel this when:
    // 1. Builder dies
    // 2. We lose access to the building
    public sealed class ConstructBuildingOrderGenerator : IOrderGenerator
    {
        private readonly ObjectDefinition _buildingDefinition;
        private readonly int _definitionIndex;
        private readonly GameObject _builder;
        private readonly GameData _config;

        public ConstructBuildingOrderGenerator(ObjectDefinition buildingDefinition, int definitionIndex, GameObject builder, GameData config)
        {
            _buildingDefinition = buildingDefinition;
            _definitionIndex = definitionIndex;
            _builder = builder;
            _config = config;
        }

        public void BuildRenderList(RenderList renderList)
        {
            // TODO draw building ghost
        }

        public OrderGeneratorResult OnActivate(Scene3D scene, Vector3 position)
        {
            var order = Order.CreateBuildObject(scene.GetPlayerIndex(scene.LocalPlayer), _definitionIndex, position, 0);

            // TODO: Check if we still have enough money
            // TODO: Check that the builder can reach target position
            // TODO: Check that the building has enough space at the target position
            // TODO: Check that the terrain is even enough at the target position

            return OrderGeneratorResult.SuccessAndExit(new[] { order });
        }
    }
}

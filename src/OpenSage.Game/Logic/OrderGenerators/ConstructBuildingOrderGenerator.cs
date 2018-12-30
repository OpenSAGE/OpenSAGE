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

            // TODO: This should work for all collider types.
            if (Collider.Create(_buildingDefinition, new Transform(position, Quaternion.Identity)) is BoxCollider collider)
            {
                // TODO: Optimize using an octree.
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

            // TODO: Check that the target area has been explored
            // TODO: Check that we still have enough money
            // TODO: Check that the builder can reach target position
            // TODO: Check that the terrain is even enough at the target position

            // TODO: Also send an order to builder to start building.
            return OrderGeneratorResult.SuccessAndExit(new[] { order });
        }
    }
}

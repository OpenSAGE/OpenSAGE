using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public static class EntityCullingExtensions
    {
        public static BoundingBox GetEnclosingBoundingBox(this Entity entity)
        {
            var renderables = entity.GetComponentsInSelfAndDescendants<RenderableComponent>();

            BoundingBox boundingBox = default(BoundingBox);

            var first = true;
            foreach (var renderable in renderables)
            {
                if (first)
                {
                    boundingBox = renderable.BoundingBox;
                    first = false;
                }
                else
                {
                    boundingBox = BoundingBox.CreateMerged(boundingBox, renderable.BoundingBox);
                }
            }

            return boundingBox;
        }
    }
}

using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class TerrainComponent : EntityComponent
    {
        internal const int PatchSize = 17;

        public HeightMap HeightMap { get; set; }

        public Vector3? Intersect(Ray ray)
        {
            if (ray.Intersects(BoundingBox) == null)
            {
                return null;
            }

            float? closestIntersection = null;

            foreach (var patchComponent in Entity.GetComponents<TerrainPatchComponent>())
            {
                patchComponent.Intersect(ray, ref closestIntersection);
            }

            if (closestIntersection == null)
            {
                return null;
            }

            return ray.Position + (ray.Direction * closestIntersection.Value);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureInfo
    {
        public uint TextureIndex;
        public uint CellSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CliffInfo
    {
        public Vector2 BottomLeftUV;
        public Vector2 BottomRightUV;
        public Vector2 TopRightUV;
        public Vector2 TopLeftUV;
    }
}

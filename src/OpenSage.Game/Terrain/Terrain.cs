using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class Terrain
    {
        private readonly ShaderSet _shaderSet;
        private readonly Pipeline _pipeline;
        private readonly ResourceSet _materialResourceSet;

        internal const int PatchSize = 17;

        public HeightMap HeightMap { get; }

        public IReadOnlyList<TerrainPatch> Patches { get; }

        public ResourceSet CloudResourceSet { get; }

        internal Terrain(
            HeightMap heightMap,
            List<TerrainPatch> patches,
            ShaderSet shaderSet,
            Pipeline pipeline,
            ResourceSet materialResourceSet,
            ResourceSet cloudResourceSet)
        {
            HeightMap = heightMap;
            Patches = patches;
            CloudResourceSet = cloudResourceSet;

            _shaderSet = shaderSet;
            _pipeline = pipeline;
            _materialResourceSet = materialResourceSet;
        }

        public Vector3? Intersect(Ray ray)
        {
            float? closestIntersection = null;

            foreach (var patch in Patches)
            {
                patch.Intersect(ray, ref closestIntersection);
            }

            if (closestIntersection == null)
            {
                return null;
            }

            return ray.Position + (ray.Direction * closestIntersection.Value);
        }

        internal void BuildRenderList(RenderList renderList)
        {
            foreach (var patch in Patches)
            {
                patch.BuildRenderList(
                    renderList,
                    _shaderSet,
                    _pipeline,
                    _materialResourceSet);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureInfo
    {
        public uint TextureIndex;
        public uint CellSize;
        public Vector2 _Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CliffInfo
    {
        public const int Size = sizeof(float) * 8;

        public Vector2 BottomLeftUV;
        public Vector2 BottomRightUV;
        public Vector2 TopRightUV;
        public Vector2 TopLeftUV;
    }
}

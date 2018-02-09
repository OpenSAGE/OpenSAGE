using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class Terrain
    {
        private readonly Texture _macroTexture;
        private readonly Texture _whiteTexture;

        internal const int PatchSize = 17;

        public HeightMap HeightMap { get; }

        public IReadOnlyList<TerrainPatch> Patches { get; }

        public Texture CloudTexture { get; }

        public Texture WhiteTexture { get; }

        public bool EnableMacroTexture { get; set; } = true;

        internal Terrain(
            HeightMap heightMap,
            List<TerrainPatch> patches,
            Texture cloudTexture,
            Texture macroTexture,
            Texture whiteTexture)
        {
            HeightMap = heightMap;
            Patches = patches;
            CloudTexture = cloudTexture;

            _macroTexture = macroTexture;
            _whiteTexture = whiteTexture;
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
                    EnableMacroTexture ? _macroTexture : _whiteTexture);
            }
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

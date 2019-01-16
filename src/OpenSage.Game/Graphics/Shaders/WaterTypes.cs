using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal static class WaterTypes
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct WaterVertex
        {
            public Vector3 Position;

            public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
                new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
        }
    }
}

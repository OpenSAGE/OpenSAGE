using System.Numerics;

namespace LLGfx
{
    public struct Viewport
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public float MinDepth;
        public float MaxDepth;

        public Viewport(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = 0;
            MaxDepth = 1;
        }

        public float AspectRatio => Width != 0 && Height != 0
            ? Width / (float) Height
            : 0;

        public Vector3 Unproject(
            Vector3 source, 
            ref Matrix4x4 projection, 
            ref Matrix4x4 view, 
            ref Matrix4x4 world)
        {
            if (!Matrix4x4.Invert(Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection), out var matrix))
            {
                return Vector3.Zero;
            }

            source.X = (((source.X - X) / Width) * 2f) - 1f;
            source.Y = -((((source.Y - Y) / Height) * 2f) - 1f);
            source.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);
            Vector3 vector = Vector3.Transform(source, matrix);
            float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector.X = vector.X / a;
                vector.Y = vector.Y / a;
                vector.Z = vector.Z / a;
            }
            return vector;
        }

        private static bool WithinEpsilon(float a, float b)
        {
            float diff = a - b;
            return (-1.401298E-45f <= diff) && (diff <= float.Epsilon);
        }
    }
}

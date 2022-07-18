using System.Numerics;
using Veldrid;

namespace OpenSage.Graphics
{
    internal static class ViewportExtensions
    {
        /// <summary>
        /// Projects a <see cref="Vector3"/> from world space into screen space.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to project.</param>
        /// <param name="projection">The projection <see cref="Matrix"/>.</param>
        /// <param name="view">The view <see cref="Matrix"/>.</param>
        /// <param name="world">The world <see cref="Matrix"/>.</param>
        /// <returns></returns>
        public static Vector3 Project(
            this Viewport viewport,
            in Vector3 source,
            in Matrix4x4 projection,
            in Matrix4x4 view,
            in Matrix4x4 world)
        {
            var matrix = world * view * projection;
            var vector = Vector3.Transform(source, matrix);
            var a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector.X = vector.X / a;
                vector.Y = vector.Y / a;
                vector.Z = vector.Z / a;
            }
            vector.X = (((vector.X + 1f) * 0.5f) * viewport.Width) + viewport.X;
            vector.Y = (((-vector.Y + 1f) * 0.5f) * viewport.Height) + viewport.Y;
            vector.Z = (vector.Z * (viewport.MaxDepth - viewport.MinDepth)) + viewport.MinDepth;
            return vector;
        }

        /// <summary>
        /// Unprojects a <see cref="Vector3"/> from screen space into world space.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to unproject.</param>
        /// <param name="projection">The projection <see cref="Matrix"/>.</param>
        /// <param name="view">The view <see cref="Matrix"/>.</param>
        /// <param name="world">The world <see cref="Matrix"/>.</param>
        /// <returns></returns>
        public static Vector3 Unproject(
            this Viewport viewport,
            Vector3 source,
            in Matrix4x4 projection,
            in Matrix4x4 view,
            in Matrix4x4 world)
        {
            if (!Matrix4x4.Invert(Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection), out var matrix))
            {
                throw new System.InvalidOperationException();
            }

            source.X = (((source.X - viewport.X) / viewport.Width) * 2f) - 1f;
            source.Y = -((((source.Y - viewport.Y) / viewport.Height) * 2f) - 1f);
            source.Z = (source.Z - viewport.MinDepth) / (viewport.MaxDepth - viewport.MinDepth);
            var vector = Vector3.Transform(source, matrix);
            var a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
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
            var diff = a - b;
            return (-1.401298E-45f <= diff) && (diff <= float.Epsilon);
        }
    }
}

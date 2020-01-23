using System;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems
{
    internal static class ParticleSystemUtility
    {
        private static readonly Random Random = new Random(1000);

        public static float GetRandomFloat(float min, float max)
        {
            return GetRandomFloat(new RandomVariable(min, max, DistributionType.Uniform));
        }

        public static float GetRandomFloat(this RandomVariable variable)
        {
            var min = variable.Low;
            var max = variable.High;

            if (min == max)
            {
                return min;
            }

            // This is how the original engine behaves.
            if (max < min)
            {
                return max;
            }

            return variable.DistributionType switch
            {
                DistributionType.Uniform => min + ((float) Random.NextDouble() * (max - min)),
                _ => throw new NotSupportedException(),
            };
        }

        public static int GetRandomInt(this RandomVariable variable)
        {
            // TODO: I don't think this will ever give us the max value.
            return (int) variable.GetRandomFloat();
        }

        public static float GetRandomAngle()
        {
            return MathUtility.TwoPi * (float) Random.NextDouble();
        }

        public static Vector2 GetRandomDirection2D()
        {
            float azimuth = GetRandomAngle();
            return new Vector2(
                MathUtility.Cos(azimuth), 
                MathUtility.Sin(azimuth));
        }

        public static Vector3 GetRandomDirection3D()
        {
            float z = (2 * (float) Random.NextDouble()) - 1; // z is in the range [-1,1]
            var planar = GetRandomDirection2D() * MathUtility.Sqrt(1 - z * z);
            return new Vector3(planar.X, planar.Y, z);
        }
    }
}

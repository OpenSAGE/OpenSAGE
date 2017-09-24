using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems.VolumeTypes
{
    internal sealed class BoxVolumeType : IVolumeType
    {
        public Ray GetRay(ParticleSystemDefinition definition)
        {
            var x = ParticleSystemUtility.GetRandomFloat(-definition.VolBoxHalfSize.X, definition.VolBoxHalfSize.X);
            var y = ParticleSystemUtility.GetRandomFloat(-definition.VolBoxHalfSize.Y, definition.VolBoxHalfSize.Y);
            var z = ParticleSystemUtility.GetRandomFloat(0, definition.VolBoxHalfSize.Z * 2);

            var position = new Vector3(x, y, z);

            return new Ray(
                position,
                Vector3.Normalize(position));
        }
    }
}

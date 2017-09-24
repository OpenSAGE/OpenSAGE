using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems.VolumeTypes
{
    internal sealed class SphereVolumeType : IVolumeType
    {
        public Ray GetRay(ParticleSystemDefinition definition)
        {
            var direction = ParticleSystemUtility.GetRandomDirection3D();

            var radius = definition.IsHollow
                ? definition.VolSphereRadius
                : ParticleSystemUtility.GetRandomFloat(0, definition.VolSphereRadius);

            return new Ray(
                direction * radius,
                direction);
        }
    }
}

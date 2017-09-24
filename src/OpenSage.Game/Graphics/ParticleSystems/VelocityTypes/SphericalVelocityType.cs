using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VelocityTypes
{
    internal sealed class SphericalVelocityType : IVelocityType
    {
        public Vector3 GetVelocity(ParticleSystemDefinition definition, Vector3 direction)
        {
            var velocity = ParticleSystemUtility.GetRandomDirection3D();
            velocity *= definition.VelSpherical.GetRandomFloat();
            return velocity;
        }
    }
}

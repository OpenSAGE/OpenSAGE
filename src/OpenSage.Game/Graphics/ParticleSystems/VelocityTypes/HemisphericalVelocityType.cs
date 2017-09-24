using System;
using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VelocityTypes
{
    internal sealed class HemisphericalVelocityType : IVelocityType
    {
        public Vector3 GetVelocity(ParticleSystemDefinition definition, Vector3 direction)
        {
            var velocity = ParticleSystemUtility.GetRandomDirection3D();
            velocity.Z = Math.Abs(velocity.Z);

            velocity *= definition.VelSpherical.GetRandomFloat();

            return velocity;
        }
    }
}

using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VelocityTypes
{
    internal sealed class NoneVelocityType : IVelocityType
    {
        public Vector3 GetVelocity(ParticleSystemDefinition definition, Vector3 direction)
        {
            return Vector3.Zero;
        }
    }
}

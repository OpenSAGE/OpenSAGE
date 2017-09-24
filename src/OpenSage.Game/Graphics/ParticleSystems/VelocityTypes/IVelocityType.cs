using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VelocityTypes
{
    internal interface IVelocityType
    {
        Vector3 GetVelocity(ParticleSystemDefinition definition, Vector3 direction);
    }
}

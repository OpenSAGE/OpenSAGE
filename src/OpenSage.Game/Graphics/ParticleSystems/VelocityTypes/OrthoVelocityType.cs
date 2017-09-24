using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VelocityTypes
{
    internal sealed class OrthoVelocityType : IVelocityType
    {
        public Vector3 GetVelocity(ParticleSystemDefinition definition, Vector3 direction)
        {
            return new Vector3(
                definition.VelOrthoX.GetRandomFloat(),
                definition.VelOrthoY.GetRandomFloat(),
                definition.VelOrthoZ.GetRandomFloat());
        }
    }
}

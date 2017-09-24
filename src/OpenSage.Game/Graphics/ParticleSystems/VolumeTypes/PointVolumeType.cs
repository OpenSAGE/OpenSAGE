using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems.VolumeTypes
{
    internal sealed class PointVolumeType : IVolumeType
    {
        public Ray GetRay(ParticleSystemDefinition definition)
        {
            return new Ray(Vector3.Zero, Vector3.Zero);
        }
    }
}

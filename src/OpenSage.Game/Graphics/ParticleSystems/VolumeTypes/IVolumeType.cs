using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems.VolumeTypes
{
    internal interface IVolumeType
    {
        Ray GetRay(ParticleSystemDefinition definition);
    }
}

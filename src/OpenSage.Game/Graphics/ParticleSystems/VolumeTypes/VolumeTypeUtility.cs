using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VolumeTypes
{
    internal static class VolumeTypeUtility
    {
        private static readonly Dictionary<ParticleVolumeType, IVolumeType> Implementations = new Dictionary<ParticleVolumeType, IVolumeType>
        {
            { ParticleVolumeType.Box, new BoxVolumeType() },
            { ParticleVolumeType.Cylinder, new CylinderVolumeType() },
            { ParticleVolumeType.Line, new LineVolumeType() },
            { ParticleVolumeType.Point, new PointVolumeType() },
            { ParticleVolumeType.Sphere, new SphereVolumeType() },
        };

        public static IVolumeType GetImplementation(ParticleVolumeType volumeType)
        {
            return Implementations[volumeType];
        }
    }
}

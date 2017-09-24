using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VelocityTypes
{
    internal static class VelocityTypeUtility
    {
        private static readonly Dictionary<ParticleVelocityType, IVelocityType> Implementations = new Dictionary<ParticleVelocityType, IVelocityType>
        {
            { ParticleVelocityType.Cylindrical, new CylindricalVelocityType() },
            { ParticleVelocityType.Hemispherical, new HemisphericalVelocityType() },
            { ParticleVelocityType.None, new NoneVelocityType() },
            { ParticleVelocityType.Ortho, new OrthoVelocityType() },
            { ParticleVelocityType.Outward, new OutwardVelocityType() },
            { ParticleVelocityType.Spherical, new SphericalVelocityType() },
        };

        public static IVelocityType GetImplementation(ParticleVelocityType velocityType)
        {
            return Implementations[velocityType];
        }
    }
}

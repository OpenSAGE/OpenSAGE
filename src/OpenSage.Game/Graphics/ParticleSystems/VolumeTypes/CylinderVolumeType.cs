using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems.VolumeTypes
{
    internal sealed class CylinderVolumeType : IVolumeType
    {
        public Ray GetRay(ParticleSystemDefinition definition)
        {
            var angle = ParticleSystemUtility.GetRandomAngle();

            var radius = definition.IsHollow
                ? definition.VolCylinderRadius
                : ParticleSystemUtility.GetRandomFloat(0, definition.VolCylinderRadius);

            var z = ParticleSystemUtility.GetRandomFloat(0, definition.VolCylinderLength);

            var direction = Vector3.Transform(
                Vector3.UnitX,
                Matrix4x4.CreateRotationZ(angle));

            return new Ray(
                new Vector3(direction.X * radius, direction.Y * radius, z),
                direction);
        }
    }
}

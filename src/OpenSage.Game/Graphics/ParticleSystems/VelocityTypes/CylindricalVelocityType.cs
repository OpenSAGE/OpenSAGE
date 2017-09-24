using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VelocityTypes
{
    internal sealed class CylindricalVelocityType : IVelocityType
    {
        public Vector3 GetVelocity(ParticleSystemDefinition definition, Vector3 direction)
        {
            var velocity = Vector3.UnitX * definition.VelCylindricalRadial.GetRandomFloat();

            velocity = Vector3.Transform(velocity, Matrix4x4.CreateRotationZ(ParticleSystemUtility.GetRandomAngle()));

            velocity.Z = definition.VelCylindricalNormal.GetRandomFloat();

            return velocity;
        }
    }
}

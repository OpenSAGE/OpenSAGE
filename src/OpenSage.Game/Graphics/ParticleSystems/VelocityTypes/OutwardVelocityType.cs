using System;
using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.ParticleSystems.VelocityTypes
{
    internal sealed class OutwardVelocityType : IVelocityType
    {
        public Vector3 GetVelocity(ParticleSystemDefinition definition, Vector3 direction)
        {
            switch (definition.VolumeType)
            {
                case ParticleVolumeType.Cylinder:
                    {
                        var velocity = direction;
                        velocity *= definition.VelOutward.GetRandomFloat();
                        velocity += Vector3.UnitZ * definition.VelOutwardOther.GetRandomFloat();
                        return velocity;
                    }

                case ParticleVolumeType.Line:
                    {
                        var up = Vector3.UnitZ;
                        if (Vector3.Dot(direction, up) <= 0.001f)
                        {
                            up = Vector3.UnitY;
                        }
                        var dir1 = Vector3.Cross(direction, up);
                        var dir2 = Vector3.Cross(dir1, dir1);
                        dir1 *= definition.VelOutward.GetRandomFloat();
                        dir2 *= definition.VelOutwardOther.GetRandomFloat();
                        return dir1 + dir2;
                    }

                case ParticleVolumeType.Point:
                    {
                        return ParticleSystemUtility.GetRandomDirection3D()
                            * definition.VelOutward.GetRandomFloat();
                    }

                case ParticleVolumeType.Box:
                case ParticleVolumeType.Sphere:
                    {
                        return direction * definition.VelOutward.GetRandomFloat();
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

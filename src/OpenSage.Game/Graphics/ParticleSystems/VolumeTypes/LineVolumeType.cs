using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Util;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.ParticleSystems.VolumeTypes
{
    internal sealed class LineVolumeType : IVolumeType
    {
        public Ray GetRay(ParticleSystemDefinition definition)
        {
            var x = ParticleSystemUtility.GetRandomFloat(definition.VolLineStart.X, definition.VolLineEnd.X);
            var y = ParticleSystemUtility.GetRandomFloat(definition.VolLineStart.Y, definition.VolLineEnd.Y);
            var z = ParticleSystemUtility.GetRandomFloat(definition.VolLineStart.Z, definition.VolLineEnd.Z);

            var position = new Vector3(x, y, z);

            var direction = Vector3.Normalize(definition.VolLineEnd.ToVector3() - definition.VolLineStart.ToVector3());

            return new Ray(position, direction);
        }
    }
}

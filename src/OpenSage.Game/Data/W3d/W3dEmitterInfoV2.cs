using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterInfoV2
    {
        public uint BurstSize { get; private set; }
        public W3dVolumeRandomizer CreationVolume { get; private set; }
        public W3dVolumeRandomizer VelRandom { get; private set; }
        public float OutwardVel { get; private set; }
        public float VelInherit { get; private set; }
        public W3dShader Shader { get; private set; }
        public W3dEmitterRenderMode RenderMode { get; private set; }
        public W3dEmitterFrameMode FrameMode { get; private set; }

        public static W3dEmitterInfoV2 Parse(BinaryReader reader)
        {
            var result = new W3dEmitterInfoV2
            {
                BurstSize = reader.ReadUInt32(),
                CreationVolume = W3dVolumeRandomizer.Parse(reader),
                VelRandom = W3dVolumeRandomizer.Parse(reader),
                OutwardVel = reader.ReadSingle(),
                VelInherit = reader.ReadSingle(),
                Shader = W3dShader.Parse(reader),
                RenderMode = reader.ReadUInt32AsEnum<W3dEmitterRenderMode>(),
                FrameMode = reader.ReadUInt32AsEnum<W3dEmitterFrameMode>()
            };

            reader.ReadBytes(6 * sizeof(uint)); // Pad

            return result;
        }
    }
}

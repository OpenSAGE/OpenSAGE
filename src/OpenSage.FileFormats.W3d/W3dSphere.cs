using System.IO;
using System.Numerics;
using System.Collections.Generic;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dSphere : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SPHERE;

        public W3dSphereHeader Header { get; private set; }

        public W3dSpherePlaceholder Placeholder { get; private set; }

        public W3dSphereColors Colors { get; private set; }

        public W3dSphereOpacityInfo OpacityInfo { get; private set; }

        public W3dSphereScaleKeyFrames ScaleKeyFrames { get; private set; }

        public W3dSphereAlphaVectors AlphaVectors { get; private set; }

        public W3dRingShaderFunc Shader { get; private set; }

        public uint UnknownFlag { get; private set; }

        internal static W3dSphere Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dSphere
                {
                    Header = W3dSphereHeader.Parse(reader)
                };

                result.Shader = (W3dRingShaderFunc) (reader.ReadUInt32() >> 24);
                result.UnknownFlag = (reader.ReadUInt32() >> 24);  // TODO: Determine What this Flag is/does.

                result.Placeholder = W3dSpherePlaceholder.Parse(reader);
                result.Colors = W3dSphereColors.Parse(reader);
                result.OpacityInfo = W3dSphereOpacityInfo.Parse(reader);
                result.ScaleKeyFrames = W3dSphereScaleKeyFrames.Parse(reader);
                result.AlphaVectors = W3dSphereAlphaVectors.Parse(reader);

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            Header.Write(writer);

            writer.Write((byte)Shader);
            writer.Write(UnknownFlag);

            Placeholder.Write(writer);
            Colors.Write(writer);
            OpacityInfo.Write(writer);
            ScaleKeyFrames.Write(writer);
            AlphaVectors.Write(writer);
        }
    }
}

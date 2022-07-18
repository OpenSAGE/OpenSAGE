using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dRing : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_RING;

        public W3dRingHeader Header { get; private set; }

        public W3dRingPlaceholder Placeholder { get; private set; }

        public W3dRingColorInfo ColorInfo { get; private set; }

        public W3dRingOpacityInfo OpacityInfo { get; private set; }

        public W3dRingScaleKeyFrames InnerScaleKeyFrames { get; private set; }

        public W3dRingScaleKeyFrames OuterScaleKeyFrames { get; private set; }

        public W3dRingShaderFunc Shader { get; private set; }

        public uint UnknownFlag { get; private set; }

        public uint TextureTiling { get; private set; }

        internal static W3dRing Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dRing
                {
                    Header = W3dRingHeader.Parse(reader)
                };

                result.Shader = (W3dRingShaderFunc) (reader.ReadUInt32() >> 24);
                result.UnknownFlag = (reader.ReadUInt32() >> 24);  // TODO: Determine What this flag is/does.

                result.Placeholder = W3dRingPlaceholder.Parse(reader);
                result.TextureTiling = reader.ReadUInt32();
                result.ColorInfo = W3dRingColorInfo.Parse(reader);
                result.OpacityInfo = W3dRingOpacityInfo.Parse(reader);
                result.InnerScaleKeyFrames = W3dRingScaleKeyFrames.Parse(reader);
                result.OuterScaleKeyFrames = W3dRingScaleKeyFrames.Parse(reader);

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            Header.Write(writer);

            writer.Write((byte) Shader);
            writer.Write(UnknownFlag);

            Placeholder.Write(writer);

            writer.Write(TextureTiling);

            ColorInfo.Write(writer);
            OpacityInfo.Write(writer);
            InnerScaleKeyFrames.Write(writer);
            OuterScaleKeyFrames.Write(writer);
        }
    }
}

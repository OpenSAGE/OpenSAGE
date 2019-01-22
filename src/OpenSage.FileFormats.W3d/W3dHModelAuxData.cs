using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dHModelAuxData : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.OBSOLETE_W3D_CHUNK_HMODEL_AUX_DATA;

        public uint Attributes { get; private set; }

        public uint MeshCount { get; private set; }

        public uint CollisionCount { get; private set; }

        public uint SkinCount { get; private set; }

        public uint[] FutureCounts { get; private set; }

        public float LodMin { get; private set; }

        public float LodMax { get; private set; }

        public byte[] FutureUse { get; private set; }

        internal static W3dHModelAuxData Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dHModelAuxData
                {
                    Attributes = reader.ReadUInt32(),
                    MeshCount = reader.ReadUInt32(),
                    CollisionCount = reader.ReadUInt32(),
                    SkinCount = reader.ReadUInt32(),
                    FutureCounts = new uint[8]
                };

                for (int i = 0; i < 8; i++)
                {
                    result.FutureCounts[i] = reader.ReadUInt32();
                }

                result.LodMin = reader.ReadSingle();
                result.LodMax = reader.ReadSingle();

                result.FutureUse = reader.ReadBytes((int) context.CurrentEndPosition - (int) reader.BaseStream.Position);

                // TODO: Determine If FutureUse are used anywhere or are different between games?

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Attributes);
            writer.Write(MeshCount);
            writer.Write(CollisionCount);
            writer.Write(SkinCount);

            for (var i = 0; i < FutureCounts.Length; i++)
            {
                writer.Write(FutureCounts[i]);
            }
           
            writer.Write(LodMin);
            writer.Write(LodMax);
            writer.Write(LodMax);
            writer.Write(FutureUse);
        }
    }
}

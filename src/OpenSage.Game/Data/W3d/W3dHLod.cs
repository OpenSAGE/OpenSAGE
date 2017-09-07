using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLod : W3dChunk
    {
        public W3dHLodHeader Header { get; private set; }

        public W3dHLodArray[] Lods { get; private set; }

        public static W3dHLod Parse(BinaryReader reader, uint chunkSize)
        {
            var currentLodIndex = 0;

            return ParseChunk<W3dHLod>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HLOD_HEADER:
                        result.Header = W3dHLodHeader.Parse(reader);
                        result.Lods = new W3dHLodArray[result.Header.LodCount];
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_LOD_ARRAY:
                        result.Lods[currentLodIndex] = W3dHLodArray.Parse(reader, header.ChunkSize);
                        currentLodIndex++;
                        break;

                    default:
                        reader.ReadBytes((int) header.ChunkSize);
                        break;
                }
            });
        }
    }

    public sealed class W3dHLodHeader
    {
        public uint Version { get; private set; }
        public uint LodCount { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Name of the hierarchy tree to use.
        /// </summary>
        public string HierarchyName { get; private set; }

        public static W3dHLodHeader Parse(BinaryReader reader)
        {
            return new W3dHLodHeader
            {
                Version = reader.ReadUInt32(),
                LodCount = reader.ReadUInt32(),
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                HierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength)
            };
        }
    }

    public sealed class W3dHLodArrayHeader
    {
        public uint ModelCount { get; private set; }

        /// <summary>
        /// If model is bigger than this, switch to higher LOD.
        /// </summary>
        public float MaxScreenSize { get; private set; }

        public static W3dHLodArrayHeader Parse(BinaryReader reader)
        {
            return new W3dHLodArrayHeader
            {
                ModelCount = reader.ReadUInt32(),
                MaxScreenSize = reader.ReadSingle()
            };
        }
    }

    public sealed class W3dHLodArray : W3dChunk
    {
        public W3dHLodArrayHeader Header { get; private set; }

        public W3dHLodSubObject[] SubObjects { get; private set; }

        public static W3dHLodArray Parse(BinaryReader reader, uint chunkSize)
        {
            var currentSubObjectIndex = 0;

            return ParseChunk<W3dHLodArray>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT_ARRAY_HEADER:
                        result.Header = W3dHLodArrayHeader.Parse(reader);
                        result.SubObjects = new W3dHLodSubObject[result.Header.ModelCount];
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT:
                        result.SubObjects[currentSubObjectIndex] = W3dHLodSubObject.Parse(reader);
                        currentSubObjectIndex++;
                        break;

                    default:
                        reader.ReadBytes((int) header.ChunkSize);
                        break;
                }
            });
        }
    }

    public sealed class W3dHLodSubObject
    {
        public uint BoneIndex { get; private set; }

        public string Name { get; private set; }

        public static W3dHLodSubObject Parse(BinaryReader reader)
        {
            return new W3dHLodSubObject
            {
                BoneIndex = reader.ReadUInt32(),
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2)
            };
        }
    }
}

﻿using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d;

public sealed class W3dMeshHeader3 : W3dChunk
{
    public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_MESH_HEADER3;

    // boundary values for W3dMeshHeaderStruct::SortLevel
    public const int SortLevelNone = 0;
    public const int SortLevelMax = 32;
    public const int SortLevelBin1 = 20;
    public const int SortLevelBin2 = 15;
    public const int SortLevelBin3 = 10;

    public uint Version { get; private set; }

    public W3dMeshFlags Attributes { get; private set; }

    public string MeshName { get; private set; }

    public string ContainerName { get; private set; }

    //
    // Counts, these can be regarded as an inventory of what is to come in the file.
    //

    public uint NumTris { get; private set; }             // number of triangles

    public uint NumVertices { get; private set; }     // number of unique vertices

    public uint NumMaterials { get; private set; }        // number of unique materials

    public uint NumDamageStages { get; private set; } // number of damage offset chunks

    public uint SortLevel { get; private set; }           // static sorting level of this mesh

    public uint PrelitVersion { get; private set; }       // mesh generated by this version of Lightmap Tool

    public uint FutureCounts { get; private set; } // future counts

    public W3dVertexChannels VertexChannels { get; private set; }  // bits for presence of types of per-vertex info

    public W3dFaceChannels FaceChannels { get; private set; }        // bits for presence of types of per-face info

    //
    // Bounding volumes
    //
    public Vector3 Min { get; private set; }                    // Min corner of the bounding box

    public Vector3 Max { get; private set; }                    // Max corner of the bounding box

    public Vector3 SphCenter { get; private set; }          // Center of bounding sphere

    public float SphRadius { get; private set; }			// Bounding sphere radius

    internal static W3dMeshHeader3 Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            return new W3dMeshHeader3
            {
                Version = reader.ReadUInt32(),
                Attributes = reader.ReadUInt32AsEnumFlags<W3dMeshFlags>(),
                MeshName = reader.ReadFixedLengthString(W3dConstants.NameLength),
                ContainerName = reader.ReadFixedLengthString(W3dConstants.NameLength),
                NumTris = reader.ReadUInt32(),
                NumVertices = reader.ReadUInt32(),
                NumMaterials = reader.ReadUInt32(),
                NumDamageStages = reader.ReadUInt32(),
                SortLevel = reader.ReadUInt32(),
                PrelitVersion = reader.ReadUInt32(),
                FutureCounts = reader.ReadUInt32(),
                VertexChannels = reader.ReadUInt32AsEnumFlags<W3dVertexChannels>(),
                FaceChannels = reader.ReadUInt32AsEnum<W3dFaceChannels>(),
                Min = reader.ReadVector3(),
                Max = reader.ReadVector3(),
                SphCenter = reader.ReadVector3(),
                SphRadius = reader.ReadSingle()
            };
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.Write((uint)Attributes);
        writer.WriteFixedLengthString(MeshName, W3dConstants.NameLength);
        writer.WriteFixedLengthString(ContainerName, W3dConstants.NameLength);
        writer.Write(NumTris);
        writer.Write(NumVertices);
        writer.Write(NumMaterials);
        writer.Write(NumDamageStages);
        writer.Write(SortLevel);
        writer.Write(PrelitVersion);
        writer.Write(FutureCounts);
        writer.Write((uint)VertexChannels);
        writer.Write((uint)FaceChannels);
        writer.Write(Min);
        writer.Write(Max);
        writer.Write(SphCenter);
        writer.Write(SphRadius);
    }
}

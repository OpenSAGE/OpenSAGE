﻿using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Data.W3x
{
    public sealed class W3xMesh
    {
        public W3xMeshVertexData VertexData { get; private set; }
        public MeshGeometryType GeometryType { get; private set; }
        public AxisAlignedBoundingBox BoundingBox { get; private set; }
        public BoundingSphere BoundingSphere { get; private set; }
        public W3xTriangle[] Triangles { get; private set; }
        public FXShaderMaterial FXShader { get; private set; }
        public W3xAabTree AabTree { get; private set; }
        public bool Hidden { get; private set; }
        public bool CastShadow { get; private set; }
        public byte SortLevel { get; private set; }

        internal static W3xMesh Parse(BinaryReader reader, AssetImportCollection imports, AssetEntry header)
        {
            var result = new W3xMesh
            {
                VertexData = reader.ReadAtOffset(() => W3xMeshVertexData.Parse(reader, header)),
                GeometryType = reader.ReadUInt32AsEnum<MeshGeometryType>(),
                BoundingBox = new AxisAlignedBoundingBox(reader.ReadVector3(), reader.ReadVector3())
            };

            var sphereRadius = reader.ReadSingle();
            var sphereCenter = reader.ReadVector3();
            result.BoundingSphere = new BoundingSphere(sphereCenter, sphereRadius);

            result.Triangles = reader.ReadArrayAtOffset(() => W3xTriangle.Parse(reader));

            result.FXShader = FXShaderMaterial.Parse(reader, imports);

            result.AabTree = reader.ReadAtOffset(() => W3xAabTree.Parse(reader));

            result.Hidden = reader.ReadBooleanChecked();
            result.CastShadow = reader.ReadBooleanChecked();
            result.SortLevel = reader.ReadByte();

            return result;
        }
    }

    public enum MeshGeometryType : uint
    {
        Normal = 0,
        Skin,
        CameraAligned,
        CameraOriented
    }
}

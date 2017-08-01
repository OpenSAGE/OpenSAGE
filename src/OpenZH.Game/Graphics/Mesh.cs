using System.Numerics;
using System.Runtime.InteropServices;
using OpenZH.Data.W3d;
using OpenZH.Game.Util;
using OpenZH.Graphics;

namespace OpenZH.Game.Graphics
{
    public sealed class Mesh : GraphicsObject
    {
        private readonly W3dMesh _w3dMesh;

        private uint _numVertices;
        private Buffer _vertexBuffer;

        private uint _numIndices;
        private Buffer _indexBuffer;

        public Mesh(W3dMesh w3dMesh)
        {
            _w3dMesh = w3dMesh;
        }

        public void Initialize(GraphicsDevice graphicsDevice, ResourceUploadBatch uploadBatch)
        {
            CreateVertexBuffer(graphicsDevice, uploadBatch);

            CreateIndexBuffer(graphicsDevice, uploadBatch);
        }

        private void CreateVertexBuffer(GraphicsDevice graphicsDevice, ResourceUploadBatch uploadBatch)
        {
            _numVertices = (uint) _w3dMesh.Vertices.Length;

            var vertices = new MeshVertex[_numVertices];

            for (var i = 0; i < _numVertices; i++)
            {
                vertices[i] = new MeshVertex
                {
                    Position = _w3dMesh.Vertices[i].ToVector3(),
                    Normal = _w3dMesh.Normals[i].ToVector3(),

                    // TODO
                    UV = _w3dMesh.MaterialPasses[0].TextureStages[0].TexCoords[i].ToVector2()
                };
            }

            var vertexBufferSize = _numVertices * MeshVertex.SizeInBytes;

            _vertexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertices));
        }

        private void CreateIndexBuffer(GraphicsDevice graphicsDevice, ResourceUploadBatch uploadBatch)
        {
            _numIndices = (uint) _w3dMesh.Triangles.Length * 3;

            var indices = new ushort[_numIndices];
            var indexIndex = 0;
            foreach (var triangle in _w3dMesh.Triangles)
            {
                indices[indexIndex++] = (ushort) triangle.VIndex0;
                indices[indexIndex++] = (ushort) triangle.VIndex1;
                indices[indexIndex++] = (ushort) triangle.VIndex2;
            }
                                                                                                                    
            _indexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                indices));
        }

        public void Draw(CommandEncoder commandEncoder)
        {
            commandEncoder.SetVertexBuffer(0, _vertexBuffer);

            commandEncoder.DrawIndexed(
                PrimitiveType.TriangleList,
                _numIndices,
                IndexType.UInt16,
                _indexBuffer,
                0);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshVertex
        {
            public const int SizeInBytes = sizeof(float) * 8;

            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;
        }
    }
}

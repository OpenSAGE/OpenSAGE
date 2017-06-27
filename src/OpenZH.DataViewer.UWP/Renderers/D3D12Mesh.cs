using System;
using System.Linq;
using OpenZH.Data.W3d;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace OpenZH.DataViewer.UWP.Renderers
{
    public class D3D12Mesh : IDisposable
    {
        private readonly W3dMesh _w3dMesh;

        private Resource _indexBuffer;
        private IndexBufferView _indexBufferView;

        private Resource _positionVertexBuffer;
        private VertexBufferView _positionVertexBufferView;

        private Resource _normalVertexBuffer;
        private VertexBufferView _normalVertexBufferView;

        private Resource _texCoordVertexBuffer;
        private VertexBufferView _texCoordVertexBufferView;

        public D3D12Mesh(W3dMesh w3dMesh)
        {
            _w3dMesh = w3dMesh;
        }

        public void Initialize(Device device)
        {
            void createVertexBuffer<T>(T[] vertexData, out Resource vertexBuffer, out VertexBufferView vertexBufferView)
                where T : struct
            {
                var vertexBufferSize = Utilities.SizeOf(vertexData);

                // Note: using upload heaps to transfer static data like vert buffers is not 
                // recommended. Every time the GPU needs it, the upload heap will be marshalled 
                // over. Please read up on Default Heap usage. An upload heap is used here for 
                // code simplicity and because there are very few verts to actually transfer.
                vertexBuffer = device.CreateCommittedResource(
                    new HeapProperties(HeapType.Upload),
                    HeapFlags.None,
                    ResourceDescription.Buffer(vertexBufferSize),
                    ResourceStates.GenericRead);

                // Copy the triangle data to the vertex buffer.
                var pVertexDataBegin = vertexBuffer.Map(0);
                Utilities.Write(pVertexDataBegin, vertexData, 0, vertexData.Length);
                vertexBuffer.Unmap(0);

                // Initialize the vertex buffer view.
                vertexBufferView = new VertexBufferView();
                vertexBufferView.BufferLocation = vertexBuffer.GPUVirtualAddress;
                vertexBufferView.StrideInBytes = Utilities.SizeOf<T>();
                vertexBufferView.SizeInBytes = vertexBufferSize;
            }

            createVertexBuffer(_w3dMesh.Vertices, out _positionVertexBuffer, out _positionVertexBufferView);
            createVertexBuffer(_w3dMesh.Normals, out _normalVertexBuffer, out _normalVertexBufferView);

            //createVertexBuffer(_w3dMesh.TexCoords, out _texCoordVertexBuffer, out _texCoordVertexBufferView);

            var indexBufferSize = sizeof(uint) * _w3dMesh.Triangles.Length * 3;

            _indexBuffer = device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(indexBufferSize),
                ResourceStates.GenericRead);

            var indices = _w3dMesh.Triangles
                .SelectMany(x => new[] { x.VIndex0, x.VIndex1, x.VIndex2 })
                .ToArray();

            var indexDataBegin = _indexBuffer.Map(0);
            Utilities.Write(indexDataBegin, indices, 0, indices.Length);
            _indexBuffer.Unmap(0);

            _indexBufferView = new IndexBufferView();
            _indexBufferView.BufferLocation = _indexBuffer.GPUVirtualAddress;
            _indexBufferView.Format = SharpDX.DXGI.Format.R32_UInt;
            _indexBufferView.SizeInBytes = indexBufferSize;
        }

        public void UpdateMaterial(ref W3dMeshViewRenderer.MaterialConstantBuffer material)
        {
            var vertexMaterial = _w3dMesh.Materials[0].VertexMaterialInfo;

            material.MaterialAmbient = vertexMaterial.Ambient.ToRawVector3();
            material.MaterialDiffuse = vertexMaterial.Diffuse.ToRawVector3();
            material.MaterialSpecular = vertexMaterial.Specular.ToRawVector3();
            material.MaterialEmissive = vertexMaterial.Emissive.ToRawVector3();
            material.MaterialShininess = vertexMaterial.Shininess;
            material.MaterialOpacity = vertexMaterial.Opacity;
        }

        public void Draw(GraphicsCommandList commandList)
        {
            commandList.PrimitiveTopology = PrimitiveTopology.TriangleList;
            commandList.SetVertexBuffer(0, _positionVertexBufferView);
            commandList.SetVertexBuffer(1, _normalVertexBufferView);
            //commandList.SetVertexBuffer(2, _texCoordVertexBufferView);
            commandList.SetIndexBuffer(_indexBufferView);
            commandList.DrawIndexedInstanced(_w3dMesh.Triangles.Length * 3, 1, 0, 0, 0);
        }

        public void Dispose()
        {
            _indexBuffer?.Dispose();
            _positionVertexBuffer?.Dispose();
        }
    }
}

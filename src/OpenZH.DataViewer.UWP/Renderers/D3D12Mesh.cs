using System;
using System.Linq;
using OpenZH.Data.W3d;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;

namespace OpenZH.DataViewer.UWP.Renderers
{
    public class D3D12Mesh : IDisposable
    {
        private readonly W3dMesh _w3dMesh;

        private Resource _indexBuffer;
        private IndexBufferView _indexBufferView;

        private Resource _positionVertexBuffer;
        private VertexBufferView _positionVertexBufferView;

        public D3D12Mesh(W3dMesh w3dMesh)
        {
            _w3dMesh = w3dMesh;
        }

        public void Initialize(Device device)
        {
            var positionVertexBufferSize = Utilities.SizeOf(_w3dMesh.Vertices);

            // Note: using upload heaps to transfer static data like vert buffers is not 
            // recommended. Every time the GPU needs it, the upload heap will be marshalled 
            // over. Please read up on Default Heap usage. An upload heap is used here for 
            // code simplicity and because there are very few verts to actually transfer.
            _positionVertexBuffer = device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(positionVertexBufferSize),
                ResourceStates.GenericRead);

            // Copy the triangle data to the vertex buffer.
            var pVertexDataBegin = _positionVertexBuffer.Map(0);
            Utilities.Write(pVertexDataBegin, _w3dMesh.Vertices, 0, _w3dMesh.Vertices.Length);
            _positionVertexBuffer.Unmap(0);

            // Initialize the vertex buffer view.
            _positionVertexBufferView = new VertexBufferView();
            _positionVertexBufferView.BufferLocation = _positionVertexBuffer.GPUVirtualAddress;
            _positionVertexBufferView.StrideInBytes = Utilities.SizeOf<W3dVector>();
            _positionVertexBufferView.SizeInBytes = positionVertexBufferSize;

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

        public void Draw(GraphicsCommandList commandList)
        {
            commandList.PrimitiveTopology = PrimitiveTopology.TriangleList;
            commandList.SetVertexBuffer(0, _positionVertexBufferView);
            commandList.SetIndexBuffer(_indexBufferView);
            commandList.DrawIndexedInstanced(3, _w3dMesh.Triangles.Length, 0, 0, 0);
        }

        public void Dispose()
        {
            _indexBuffer?.Dispose();
            _positionVertexBuffer?.Dispose();
        }
    }
}

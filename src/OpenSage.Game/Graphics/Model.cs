using OpenSage.Data.W3d;
using LLGfx;
using System.Collections.Generic;
using OpenSage.Data;

namespace OpenSage.Graphics
{
    public sealed class Model : GraphicsObject
    {
        public IReadOnlyList<ModelMesh> Meshes { get; }

        internal Model(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dFile w3dFile,
            FileSystem fileSystem,
            DescriptorSetLayout pixelMeshDescriptorSetLayout,
            DescriptorSetLayout vertexMaterialPassDescriptorSetLayout,
            DescriptorSetLayout pixelMaterialPassDescriptorSetLayout)
        {
            var meshes = new List<ModelMesh>();

            foreach (var w3dMesh in w3dFile.Meshes)
            {
                var mesh = new ModelMesh(
                    graphicsDevice, 
                    uploadBatch, 
                    w3dMesh,
                    fileSystem,
                    pixelMeshDescriptorSetLayout,
                    vertexMaterialPassDescriptorSetLayout,
                    pixelMaterialPassDescriptorSetLayout);
                meshes.Add(mesh);
            }

            Meshes = meshes;
        }

        public void Draw(CommandEncoder commandEncoder)
        {
            foreach (var mesh in Meshes)
            {
                mesh.Draw(commandEncoder);
            }
        }
    }
}

using System.Numerics;
using System.Runtime.InteropServices;
using OpenZH.Data.W3d;
using OpenZH.Graphics.Util;
using OpenZH.Graphics.LowLevel;

namespace OpenZH.Graphics
{
    public sealed class MeshMaterial
    {
        private readonly W3dMesh _w3dMesh;
        private StaticBuffer _vertexMaterialsBuffer;

        public MeshMaterial(W3dMesh w3dMesh)
        {
            _w3dMesh = w3dMesh;
        }

        public void Initialize(GraphicsDevice graphicsDevice, ResourceUploadBatch uploadBatch)
        {
            var vertexMaterials = new VertexMaterial[_w3dMesh.Materials.Length];
            for (var i = 0; i < vertexMaterials.Length; i++)
            {
                var w3dVertexMaterial = _w3dMesh.Materials[i].VertexMaterialInfo;

                vertexMaterials[i] = new VertexMaterial
                {
                    // TODO: Attributes?
                    MaterialAmbient = w3dVertexMaterial.Ambient.ToVector3(),
                    MaterialDiffuse = w3dVertexMaterial.Diffuse.ToVector3(),
                    MaterialSpecular = w3dVertexMaterial.Specular.ToVector3(),
                    MaterialEmissive = w3dVertexMaterial.Emissive.ToVector3(),
                    MaterialShininess = w3dVertexMaterial.Shininess,
                    MaterialOpacity = w3dVertexMaterial.Opacity
                    // TODO: Translucency?
                };
            }

            _vertexMaterialsBuffer = StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertexMaterials);

            //var materialConstantBuffer = StaticBuffer.Create(
            //    graphicsDevice,
            //    uploadBatch,
            //    new MaterialConstants
            //    {
            //        MaterialAmbient = vertexMaterial.Ambient.ToVector3(),
            //        MaterialDiffuse = vertexMaterial.Diffuse.ToVector3(),
            //        MaterialSpecular = vertexMaterial.Specular.ToVector3(),
            //        MaterialEmissive = vertexMaterial.Emissive.ToVector3(),
            //        MaterialShininess = vertexMaterial.Shininess,
            //        MaterialOpacity = vertexMaterial.Opacity
            //    });
        }

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        private struct VertexMaterial
        {
            public const int SizeInBytes = 64;

            [FieldOffset(0)]
            public Vector3 MaterialAmbient;

            [FieldOffset(16)]
            public Vector3 MaterialDiffuse;

            [FieldOffset(32)]
            public Vector3 MaterialSpecular;

            [FieldOffset(44)]
            public float MaterialShininess;

            [FieldOffset(48)]
            public Vector3 MaterialEmissive;

            [FieldOffset(60)]
            public float MaterialOpacity;
        }
    }

    public sealed class MeshMaterialPass
    {
        public MeshMaterialPass(W3dMaterialPass w3dMaterialPass)
        {
            //w3dMaterialPass.
        }
    }
}

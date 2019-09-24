using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content.Loaders;
using OpenSage.Data.StreamFS;
using OpenSage.Data.W3x;
using OpenSage.Graphics.Shaders;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    partial class ModelMesh
    {
        internal ModelMesh(W3xMesh w3xMesh, Asset asset, AssetLoadContext loadContext)
        {
            SetNameAndInstanceId(asset);

            var effectName = w3xMesh.FXShader.ShaderName.Replace(".fx", string.Empty);
            //var shaderResources = loadContext.ShaderResources.GetShaderMaterialResources(effectName); // TODO: Uncomment this.
            var shaderResources = loadContext.ShaderResources.GetShaderMaterialResources("NormalMapped");
            _depthPipeline = loadContext.ShaderResources.MeshDepth.TriangleListPipeline;

            // TODO: Extract state properties from shader material.

            var pipeline = shaderResources.Pipeline;

            // TODO: Uncomment this.

            //var materialResourceSetBuilder = AddDisposable(new ShaderMaterialResourceSetBuilder(
            //    loadContext.GraphicsDevice,
            //    shaderResources));

            //foreach (var shaderConstant in w3xMesh.FXShader.Constants)
            //{
            //    switch (shaderConstant.Type)
            //    {
            //        case FXShaderConstantType.Float:
            //            switch (shaderConstant.FloatValueCount)
            //            {
            //                case 1:
            //                    materialResourceSetBuilder.SetConstant(shaderConstant.Name, shaderConstant.Value.Float);
            //                    break;

            //                case 2:
            //                    materialResourceSetBuilder.SetConstant(shaderConstant.Name, shaderConstant.Value.Vector2);
            //                    break;

            //                case 3:
            //                    materialResourceSetBuilder.SetConstant(shaderConstant.Name, shaderConstant.Value.Vector3);
            //                    break;

            //                case 4:
            //                    materialResourceSetBuilder.SetConstant(shaderConstant.Name, shaderConstant.Value.Vector4);
            //                    break;

            //                default:
            //                    throw new InvalidOperationException();
            //            }
            //            break;

            //        case FXShaderConstantType.Bool:
            //            materialResourceSetBuilder.SetConstant(shaderConstant.Name, shaderConstant.Value.Bool);
            //            break;

            //        case FXShaderConstantType.Int:
            //            materialResourceSetBuilder.SetConstant(shaderConstant.Name, shaderConstant.Value.Int);
            //            break;

            //        case FXShaderConstantType.Texture:
            //            materialResourceSetBuilder.SetTexture(shaderConstant.Name, shaderConstant.TextureValue);
            //            break;

            //        default:
            //            throw new NotSupportedException();
            //    }
            //}

            //var materialResourceSet = materialResourceSetBuilder.CreateResourceSet();

            var materialResourceSet = (ResourceSet) null;

            MeshParts = new List<ModelMeshPart>();
            MeshParts.Add(new ModelMeshPart(
                null,
                0,
                (uint)(w3xMesh.Triangles.Length * 3),
                false, // TODO
                pipeline,
                materialResourceSet));

            BoundingBox = w3xMesh.BoundingBox;

            _shaderSet = shaderResources.ShaderSet;

            Skinned = w3xMesh.GeometryType == MeshGeometryType.Skin;
            Hidden = w3xMesh.Hidden;
            CameraOriented = w3xMesh.GeometryType == MeshGeometryType.CameraOriented;

            _vertexBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                w3xMesh.VertexData.VertexData,
                BufferUsage.VertexBuffer));

            _indexBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                CreateIndices(w3xMesh),
                BufferUsage.IndexBuffer));

            var hasHouseColor = false; // TODO
            _meshConstantsResourceSet = loadContext.ShaderResources.Mesh.GetCachedMeshResourceSet(
                Skinned,
                hasHouseColor);

            PostInitialize(loadContext);
        }

        private static ushort[] CreateIndices(W3xMesh w3xMesh)
        {
            var triangles = w3xMesh.Triangles;
            var indices = new ushort[(uint) triangles.Length * 3];

            var indexIndex = 0;
            for (var i = 0; i < triangles.Length; i++)
            {
                indices[indexIndex++] = (ushort) triangles[i].VIndex0;
                indices[indexIndex++] = (ushort) triangles[i].VIndex1;
                indices[indexIndex++] = (ushort) triangles[i].VIndex2;
            }

            return indices;
        }
    }
}

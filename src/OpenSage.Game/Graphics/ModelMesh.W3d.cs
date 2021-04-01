using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content.Loaders;
using OpenSage.Content.Util;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    partial class ModelMesh
    {
        internal ModelMesh(W3dMesh w3dMesh, AssetLoadContext loadContext)
        {
            SetNameAndInstanceId("W3DMesh", w3dMesh.Header.MeshName);

            W3dShaderMaterial w3dShaderMaterial;
            ShaderResourcesBase shaderResources;
            if (w3dMesh.MaterialPasses.Count == 1 && w3dMesh.MaterialPasses[0].ShaderMaterialIds != null)
            {
                if (w3dMesh.MaterialPasses[0].ShaderMaterialIds.Items.Length > 1)
                {
                    throw new NotSupportedException();
                }
                var shaderMaterialID = w3dMesh.MaterialPasses[0].ShaderMaterialIds.Items[0];
                w3dShaderMaterial = w3dMesh.ShaderMaterials.Items[(int) shaderMaterialID];
                var effectName = w3dShaderMaterial.Header.TypeName.Replace(".fx", string.Empty);

                shaderResources = loadContext.ShaderResources.GetShaderMaterialResources(effectName);
            }
            else
            {
                w3dShaderMaterial = null;
                shaderResources = loadContext.ShaderResources.FixedFunction;
            }

            _depthPipeline = loadContext.ShaderResources.MeshDepth.TriangleListPipeline;

            MeshParts = new List<ModelMeshPart>();
            if (w3dShaderMaterial != null)
            {
                MeshParts.Add(CreateModelMeshPartShaderMaterial(
                    loadContext,
                    w3dMesh,
                    w3dMesh.MaterialPasses[0],
                    w3dShaderMaterial,
                    (ShaderMaterialShaderResources) shaderResources));
            }
            else
            {
                var vertexMaterials = CreateMaterials(w3dMesh);

                var shadingConfigurations = new FixedFunctionShaderResources.ShadingConfiguration[w3dMesh.Shaders.Items.Count];
                for (var i = 0; i < shadingConfigurations.Length; i++)
                {
                    shadingConfigurations[i] = CreateShadingConfiguration(w3dMesh.Shaders.Items[i]);
                }

                for (var i = 0; i < w3dMesh.MaterialPasses.Count; i++)
                {
                    CreateModelMeshPartsFixedFunction(
                        loadContext,
                        w3dMesh,
                        w3dMesh.MaterialPasses[i],
                        vertexMaterials,
                        shadingConfigurations,
                        MeshParts);
                }
            }

            BoundingBox = new AxisAlignedBoundingBox(
                w3dMesh.Header.Min,
                w3dMesh.Header.Max);

            _shaderSet = shaderResources.ShaderSet;

            Skinned = w3dMesh.IsSkinned;
            Hidden = w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.Hidden);
            CameraOriented = (w3dMesh.Header.Attributes & W3dMeshFlags.GeometryTypeMask) == W3dMeshFlags.GeometryTypeCameraOriented;

            _vertexBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                MemoryMarshal.AsBytes(new ReadOnlySpan<MeshShaderResources.MeshVertex.Basic>(CreateVertices(w3dMesh, w3dMesh.IsSkinned))),
                BufferUsage.VertexBuffer));

            _indexBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                CreateIndices(w3dMesh),
                BufferUsage.IndexBuffer));

            var hasHouseColor = w3dMesh.Header.MeshName.StartsWith("HOUSECOLOR");
            _meshConstantsResourceSet = loadContext.ShaderResources.Mesh.GetCachedMeshResourceSet(
                Skinned,
                hasHouseColor);

            PostInitialize(loadContext);
        }

        private static FixedFunctionShaderResources.ShadingConfiguration CreateShadingConfiguration(W3dShader w3dShader)
        {
            return new FixedFunctionShaderResources.ShadingConfiguration
            {
                DiffuseLightingType = w3dShader.PrimaryGradient.ToDiffuseLightingType(),
                SpecularEnabled = w3dShader.SecondaryGradient == W3dShaderSecondaryGradient.Enable,
                TexturingEnabled = w3dShader.Texturing == W3dShaderTexturing.Enable,
                SecondaryTextureColorBlend = w3dShader.DetailColorFunc.ToSecondaryTextureBlend(),
                SecondaryTextureAlphaBlend = w3dShader.DetailAlphaFunc.ToSecondaryTextureBlend(),
                AlphaTest = w3dShader.AlphaTest == W3dShaderAlphaTest.Enable
            };
        }

        private static FixedFunctionShaderResources.VertexMaterial[] CreateMaterials(W3dMesh w3dMesh)
        {
            var vertexMaterials = new FixedFunctionShaderResources.VertexMaterial[w3dMesh.VertexMaterials.Items.Count];

            for (var i = 0; i < w3dMesh.VertexMaterials.Items.Count; i++)
            {
                var w3dMaterial = w3dMesh.VertexMaterials.Items[i];
                var w3dVertexMaterialInfo = w3dMaterial.Info;

                vertexMaterials[i] = w3dVertexMaterialInfo.ToVertexMaterial(w3dMaterial);
            }

            return vertexMaterials;
        }

        private static Texture CreateTexture(
            AssetLoadContext context,
            W3dMesh w3dMesh,
            uint? textureIndex)
        {
            if (textureIndex == null)
            {
                return null;
            }

            var w3dTexture = w3dMesh.Textures.Items[(int) textureIndex];

            if (w3dTexture.TextureInfo != null && w3dTexture.TextureInfo.FrameCount != 1)
            {
                throw new NotImplementedException();
            }

            return context.AssetStore.Textures.GetByName(w3dTexture.Name.Value);
        }

        private static MeshShaderResources.MeshVertex.Basic[] CreateVertices(
            W3dMesh w3dMesh,
            bool isSkinned)
        {
            var numVertices = (uint) w3dMesh.Vertices.Items.Length;
            var vertices = new MeshShaderResources.MeshVertex.Basic[numVertices];

            for (var i = 0; i < numVertices; i++)
            {
                vertices[i] = new MeshShaderResources.MeshVertex.Basic
                {
                    Position0 = w3dMesh.Vertices.Items[i],
                    Position1 = w3dMesh.Vertices2 != null
                        ? w3dMesh.Vertices2.Items[i]
                        : Vector3.Zero,
                    Normal0 = w3dMesh.Normals.Items[i],
                    Normal1 = w3dMesh.Normals2 != null
                        ? w3dMesh.Normals2.Items[i]
                        : Vector3.Zero,
                    Tangent = w3dMesh.Tangents != null
                        ? w3dMesh.Tangents.Items[i]
                        : Vector3.Zero,
                    Binormal = w3dMesh.Bitangents != null
                        ? w3dMesh.Bitangents.Items[i]
                        : Vector3.Zero,
                    BoneIndex0 = isSkinned
                        ? w3dMesh.Influences.Items[i].BoneIndex0
                        : 0u,
                    BoneIndex1 = isSkinned
                        ? w3dMesh.Influences.Items[i].BoneIndex1
                        : 0u,
                    BoneWeight0 = isSkinned
                        ? w3dMesh.Influences.Items[i].BoneWeight0 / 100.0f
                        : 0.0f,
                    BoneWeight1 = isSkinned
                        ? w3dMesh.Influences.Items[i].BoneWeight1 / 100.0f
                        : 0.0f,
                };
            }

            return vertices;
        }

        private static ushort[] CreateIndices(W3dMesh w3dMesh)
        {
            var triangles = w3dMesh.Triangles.Items.AsSpan();
            var indices = new ushort[(uint) triangles.Length * 3];

            var indexIndex = 0;
            foreach (ref readonly var triangle in triangles)
            {
                indices[indexIndex++] = (ushort) triangle.VIndex0;
                indices[indexIndex++] = (ushort) triangle.VIndex1;
                indices[indexIndex++] = (ushort) triangle.VIndex2;
            }

            return indices;
        }

        private ModelMeshPart CreateModelMeshPartShaderMaterial(
            AssetLoadContext context,
            W3dMesh w3dMesh,
            W3dMaterialPass w3dMaterialPass,
            W3dShaderMaterial w3dShaderMaterial,
            ShaderMaterialShaderResources shaderResources)
        {
            var texCoords = new MeshShaderResources.MeshVertex.TexCoords[w3dMesh.Header.NumVertices];

            if (w3dMaterialPass.TexCoords != null)
            {
                for (var i = 0; i < texCoords.Length; i++)
                {
                    texCoords[i].UV0 = w3dMaterialPass.TexCoords.Items[i];
                }
            }

            // TODO: Extract state properties from shader material.

            var blendEnabled = false;

            var pipeline = shaderResources.Pipeline;

            var materialResourceSetBuilder = AddDisposable(new ShaderMaterialResourceSetBuilder(
                context.GraphicsDevice,
                shaderResources));

            foreach (var w3dShaderProperty in w3dShaderMaterial.Properties)
            {
                switch (w3dShaderProperty.PropertyType)
                {
                    case W3dShaderMaterialPropertyType.Texture:
                        var texture = context.AssetStore.Textures.GetByName(w3dShaderProperty.StringValue) ?? context.StandardGraphicsResources.PlaceholderTexture;
                        materialResourceSetBuilder.SetTexture(w3dShaderProperty.PropertyName, texture);
                        break;

                    case W3dShaderMaterialPropertyType.Bool:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Bool);
                        break;

                    case W3dShaderMaterialPropertyType.Float:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Float);
                        break;

                    case W3dShaderMaterialPropertyType.Vector2:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector2);
                        break;

                    case W3dShaderMaterialPropertyType.Vector3:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector3);
                        break;

                    case W3dShaderMaterialPropertyType.Vector4:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector4);
                        break;

                    case W3dShaderMaterialPropertyType.Int:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Int);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            var materialResourceSet = materialResourceSetBuilder.CreateResourceSet();

            var texCoordsVertexBuffer = AddDisposable(context.GraphicsDevice.CreateStaticBuffer(
                texCoords,
                BufferUsage.VertexBuffer));

            return new ModelMeshPart(
                texCoordsVertexBuffer,
                0,
                (uint)w3dMesh.Triangles.Items.Length * 3,
                blendEnabled,
                pipeline,
                pipeline, // TODO
                materialResourceSet);
        }

        // One ModelMeshMaterialPass for each W3D_CHUNK_MATERIAL_PASS
        private void CreateModelMeshPartsFixedFunction(
            AssetLoadContext context,
            W3dMesh w3dMesh,
            W3dMaterialPass w3dMaterialPass,
            FixedFunctionShaderResources.VertexMaterial[] vertexMaterials,
            FixedFunctionShaderResources.ShadingConfiguration[] shadingConfigurations,
            List<ModelMeshPart> meshParts)
        {
            var hasTextureStage0 = w3dMaterialPass.TextureStages.Count > 0;
            var textureStage0 = hasTextureStage0
                ? w3dMaterialPass.TextureStages[0]
                : null;

            var hasTextureStage1 = w3dMaterialPass.TextureStages.Count > 1 && w3dMaterialPass.TextureStages[1].TexCoords != null;
            var textureStage1 = hasTextureStage1
                ? w3dMaterialPass.TextureStages[1]
                : null;

            var numTextureStages = hasTextureStage0 && hasTextureStage1
                ? 2u
                : hasTextureStage0 ? 1u : 0u;

            var texCoords = new MeshShaderResources.MeshVertex.TexCoords[w3dMesh.Header.NumVertices];

            if (hasTextureStage0)
            {
                for (var i = 0; i < texCoords.Length; i++)
                {
                    // TODO: What to do when this is null?
                    if (textureStage0.TexCoords != null)
                    {
                        texCoords[i].UV0 = textureStage0.TexCoords.Items[i];
                    }

                    if (hasTextureStage1)
                    {
                        texCoords[i].UV1 = textureStage1.TexCoords.Items[i];
                    }
                }
            }

            var texCoordsVertexBuffer = AddDisposable(context.GraphicsDevice.CreateStaticBuffer(
                texCoords,
                BufferUsage.VertexBuffer));

            // Optimisation for a fairly common case.
            if (w3dMaterialPass.VertexMaterialIds.Items.Length == 1
                && w3dMaterialPass.ShaderIds.Items.Length == 1
                && w3dMaterialPass.TextureStages.Count == 1
                && w3dMaterialPass.TextureStages[0].TextureIds.Items.Count == 1)
            {
                meshParts.Add(CreateModelMeshPart(
                    context,
                    texCoordsVertexBuffer,
                    0,
                    w3dMesh.Header.NumTris * 3,
                    w3dMesh,
                    vertexMaterials,
                    shadingConfigurations,
                    w3dMaterialPass.VertexMaterialIds.Items[0],
                    w3dMaterialPass.ShaderIds.Items[0],
                    numTextureStages,
                    w3dMaterialPass.TextureStages[0].TextureIds.Items[0],
                    null));
            }
            else
            {
                // Expand ShaderIds and TextureIds, if they have a single entry
                // (which means same ID for all faces)

                IEnumerable<uint?> getExpandedTextureIds(List<uint?> ids)
                {
                    if (ids == null)
                    {
                        for (var i = 0; i < w3dMesh.Header.NumTris; i++)
                        {
                            yield return null;
                        }
                    }
                    else if (ids.Count == 1)
                    {
                        var result = ids[0];
                        for (var i = 0; i < w3dMesh.Header.NumTris; i++)
                        {
                            yield return result;
                        }
                    }
                    else
                    {
                        foreach (var id in ids)
                        {
                            yield return id;
                        }
                    }
                }

                IEnumerable<uint> getExpandedShaderIds()
                {
                    var ids = w3dMaterialPass.ShaderIds.Items;
                    if (ids.Length == 1)
                    {
                        var result = ids[0];
                        for (var i = 0; i < w3dMesh.Header.NumTris; i++)
                        {
                            yield return result;
                        }
                    }
                    else
                    {
                        foreach (var id in ids)
                        {
                            yield return id;
                        }
                    }
                }

                IEnumerable<uint> getExpandedVertexMaterialIDs()
                {
                    var ids = w3dMaterialPass.VertexMaterialIds.Items;
                    if (ids.Length == 1)
                    {
                        var result = ids[0];
                        for (var i = 0; i < w3dMesh.Header.NumTris; i++)
                        {
                            yield return result;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < w3dMesh.Header.NumTris; i++)
                        {
                            var triangle = w3dMesh.Triangles.Items[i];
                            var materialID0 = ids[(int) triangle.VIndex0];
                            var materialID1 = ids[(int) triangle.VIndex1];
                            var materialID2 = ids[(int) triangle.VIndex2];
                            if (materialID0 != materialID1 || materialID1 != materialID2)
                            {
                                throw new NotSupportedException();
                            }
                            yield return materialID0;
                        }

                        foreach (var id in ids)
                        {
                            yield return id;
                        }
                    }
                }

                var combinedIds = getExpandedVertexMaterialIDs()
                    .Zip(getExpandedShaderIds(), (x, y) => new { VertexMaterialID = x, ShaderID = y })
                    .Zip(getExpandedTextureIds(textureStage0?.TextureIds.Items), (x, y) => new { x.VertexMaterialID, x.ShaderID, TextureIndex0 = y })
                    .Zip(getExpandedTextureIds(textureStage1?.TextureIds.Items), (x, y) => new CombinedMaterialPermutation { VertexMaterialID = x.VertexMaterialID, ShaderID = x.ShaderID, TextureIndex0 = x.TextureIndex0, TextureIndex1 = y });

                var combinedId = combinedIds.First();
                var startIndex = 0u;
                var indexCount = 0u;

                foreach (var newCombinedId in combinedIds)
                {
                    if (combinedId != newCombinedId)
                    {
                        meshParts.Add(CreateModelMeshPart(
                            context,
                            texCoordsVertexBuffer,
                            startIndex,
                            indexCount,
                            w3dMesh,
                            vertexMaterials,
                            shadingConfigurations,
                            combinedId.VertexMaterialID,
                            combinedId.ShaderID,
                            numTextureStages,
                            combinedId.TextureIndex0,
                            combinedId.TextureIndex1));

                        startIndex = startIndex + indexCount;
                        indexCount = 0;
                    }

                    combinedId = newCombinedId;

                    indexCount += 3;
                }

                if (indexCount > 0)
                {
                    meshParts.Add(CreateModelMeshPart(
                        context,
                        texCoordsVertexBuffer,
                        startIndex,
                        indexCount,
                        w3dMesh,
                        vertexMaterials,
                        shadingConfigurations,
                        combinedId.VertexMaterialID,
                        combinedId.ShaderID,
                        numTextureStages,
                        combinedId.TextureIndex0,
                        combinedId.TextureIndex1));
                }
            }
        }

        private struct CombinedMaterialPermutation
        {
            public uint VertexMaterialID;
            public uint ShaderID;
            public uint? TextureIndex0;
            public uint? TextureIndex1;

            public override bool Equals(object obj)
            {
                if (!(obj is CombinedMaterialPermutation))
                {
                    return false;
                }

                var permutation = (CombinedMaterialPermutation) obj;

                return this == permutation;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    VertexMaterialID,
                    ShaderID,
                    TextureIndex0,
                    TextureIndex1);
            }

            public static bool operator ==(CombinedMaterialPermutation l, CombinedMaterialPermutation r)
            {
                return l.VertexMaterialID == r.VertexMaterialID
                    && l.ShaderID == r.ShaderID
                    && l.TextureIndex0 == r.TextureIndex0
                    && l.TextureIndex1 == r.TextureIndex1;
            }

            public static bool operator !=(CombinedMaterialPermutation l, CombinedMaterialPermutation r) => !(l == r);
        }

        // One ModelMeshPart for each unique shader in a W3D_CHUNK_MATERIAL_PASS.
        private ModelMeshPart CreateModelMeshPart(
            AssetLoadContext context,
            DeviceBuffer texCoordsVertexBuffer,
            uint startIndex,
            uint indexCount,
            W3dMesh w3dMesh,
            FixedFunctionShaderResources.VertexMaterial[] vertexMaterials,
            FixedFunctionShaderResources.ShadingConfiguration[] shadingConfigurations,
            uint vertexMaterialID,
            uint shaderID,
            uint numTextureStages,
            uint? textureIndex0,
            uint? textureIndex1)
        {
            var w3dShader = w3dMesh.Shaders.Items[(int) shaderID];

            var cullMode = w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.TwoSided)
                ? FaceCullMode.None
                : FaceCullMode.Back;

            var depthWriteEnabled = w3dShader.DepthMask == W3dShaderDepthMask.WriteEnable;
            var depthComparison = w3dShader.DepthCompare.ToComparison();

            var blendEnabled = w3dShader.SrcBlend != W3dShaderSrcBlendFunc.One || w3dShader.DestBlend != W3dShaderDestBlendFunc.Zero;
            var sourceFactor = w3dShader.SrcBlend.ToBlend();
            var destinationColorFactor = w3dShader.DestBlend.ToBlend(false);
            var destinationAlphaFactor = w3dShader.DestBlend.ToBlend(true);

            var pipeline = context.ShaderResources.FixedFunction.GetCachedPipeline(
                cullMode,
                depthWriteEnabled,
                depthComparison,
                blendEnabled,
                sourceFactor,
                destinationColorFactor,
                destinationAlphaFactor);

            var pipelineBlend = context.ShaderResources.FixedFunction.GetCachedPipeline(
                cullMode,
                depthWriteEnabled,
                depthComparison,
                true,
                BlendFactor.SourceAlpha,
                BlendFactor.InverseSourceAlpha,
                BlendFactor.InverseSourceAlpha);

            var materialConstantsBuffer = AddDisposable(context.GraphicsDevice.CreateStaticBuffer(
                new FixedFunctionShaderResources.MaterialConstantsType
                {
                    Material = vertexMaterials[vertexMaterialID],
                    Shading = shadingConfigurations[shaderID],
                    NumTextureStages = (int) numTextureStages
                },
                BufferUsage.UniformBuffer));

            var materialResourceSet = AddDisposable(context.ShaderResources.FixedFunction.CreateMaterialResourceSet(
                materialConstantsBuffer,
                CreateTexture(context, w3dMesh, textureIndex0) ?? context.StandardGraphicsResources.NullTexture,
                CreateTexture(context, w3dMesh, textureIndex1) ?? context.StandardGraphicsResources.NullTexture));

            return new ModelMeshPart(
                texCoordsVertexBuffer,
                startIndex,
                indexCount,
                blendEnabled,
                pipeline,
                pipelineBlend,
                materialResourceSet);
        }
    }
}

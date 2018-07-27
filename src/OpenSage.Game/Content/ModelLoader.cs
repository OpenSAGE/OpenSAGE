using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content.Util;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Content
{
    internal sealed class ModelLoader : ContentLoader<Model>
    {
        private readonly MeshDepthMaterial _meshDepthMaterialFixedFunction;
        private readonly MeshDepthMaterial _meshDepthMaterialShader;

        public ModelLoader(ContentManager contentManager)
        {
            _meshDepthMaterialFixedFunction = AddDisposable(new MeshDepthMaterial(contentManager, contentManager.EffectLibrary.MeshDepthFixedFunction));
            _meshDepthMaterialShader = AddDisposable(new MeshDepthMaterial(contentManager, contentManager.EffectLibrary.MeshDepthShaderMaterial));
        }

        protected override Model LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            var w3dFile = W3dFile.FromFileSystemEntry(entry);

            var w3dHLod = w3dFile.GetHLod();
            var w3dHierarchy = w3dFile.GetHierarchy();
            if (w3dHLod != null && w3dHierarchy == null)
            {
                // Load referenced hierarchy.
                var hierarchyFileName = w3dHLod.Header.HierarchyName + ".W3D";
                var hierarchyFilePath = Path.Combine(Path.GetDirectoryName(w3dFile.FilePath), hierarchyFileName);
                var hierarchyFileEntry = contentManager.FileSystem.GetFile(hierarchyFilePath);
                var hierarchyFile = W3dFile.FromFileSystemEntry(hierarchyFileEntry);
                w3dHierarchy = hierarchyFile.GetHierarchy();
            }

            return CreateModel(
                contentManager,
                w3dFile,
                w3dHierarchy);
        }

        private Model CreateModel(
            ContentManager contentManager,
            W3dFile w3dFile,
            W3dHierarchyDef w3dHierarchy)
        {
            ModelBone[] bones;
            if (w3dHierarchy != null)
            {
                bones = new ModelBone[w3dHierarchy.Pivots.Items.Count];

                for (var i = 0; i < w3dHierarchy.Pivots.Items.Count; i++)
                {
                    var pivot = w3dHierarchy.Pivots.Items[i];

                    var parent = pivot.ParentIdx == -1
                        ? null
                        : bones[pivot.ParentIdx];

                    bones[i] = new ModelBone(
                        i,
                        pivot.Name,
                        parent,
                        pivot.Translation,
                        pivot.Rotation);
                }
            }
            else
            {
                bones = new ModelBone[1];
                bones[0] = new ModelBone(0, null, null, Vector3.Zero, Quaternion.Identity);
            }

            //BoundingSphere boundingSphere = default(BoundingSphere);

            var w3dMeshes = w3dFile.GetMeshes();
            var w3dHLod = w3dFile.GetHLod();

            var subObjects = new List<ModelSubObject>();

            if (w3dHLod != null)
            {
                foreach (var w3dSubObject in w3dHLod.Lods[0].SubObjects)
                {
                    // TODO: Collision boxes
                    var w3dMesh = w3dMeshes.FirstOrDefault(x => x.Header.ContainerName + "." + x.Header.MeshName == w3dSubObject.Name);
                    if (w3dMesh == null)
                    {
                        continue;
                    }

                    var bone = bones[(int) w3dSubObject.BoneIndex];

                    var mesh = CreateModelMesh(
                        contentManager,
                        w3dMesh);

                    //var meshBoundingSphere = mesh.BoundingSphere.Transform(bone.Transform);

                    //boundingSphere = (i == 0)
                    //    ? meshBoundingSphere
                    //    : BoundingSphere.CreateMerged(boundingSphere, meshBoundingSphere);

                    subObjects.Add(new ModelSubObject(w3dSubObject.Name, bone, mesh));
                }
            }
            else
            {
                // Simple models can have only one mesh with no HLod chunk.
                if (w3dMeshes.Count != 1)
                {
                    throw new InvalidOperationException();
                }

                var w3dMesh = w3dMeshes[0];

                var mesh = CreateModelMesh(
                    contentManager,
                    w3dMesh);

                subObjects.Add(new ModelSubObject(
                    w3dMesh.Header.MeshName,
                    bones[0],
                    mesh));
            }

            LoadAnimations(w3dFile, contentManager);

            return new Model(
                new ModelBoneHierarchy(bones),
                subObjects.ToArray());
        }

        public static Animation[] LoadAnimations(W3dFile w3dFile, ContentManager contentManager)
        {
            var w3dAnimations = w3dFile.GetAnimations();
            var w3dCompressedAnimations = w3dFile.GetCompressedAnimations();

            var animations = new Animation[w3dAnimations.Count + w3dCompressedAnimations.Count];
            for (var i = 0; i < w3dAnimations.Count; i++)
            {
                animations[i] = CreateAnimation(w3dAnimations[i]);
            }
            for (var i = 0; i < w3dCompressedAnimations.Count; i++)
            {
                animations[w3dAnimations.Count + i] = CreateAnimation(w3dCompressedAnimations[i]);
            }

            foreach (var animation in animations)
            {
                contentManager.DataContext.Animations.Add(animation.Name, animation);
            }

            return animations;
        }

        private ModelMesh CreateModelMesh(
            ContentManager contentManager,
            W3dMesh w3dMesh)
        {
            W3dShaderMaterial w3dShaderMaterial;
            Effect effect = null;
            if (w3dMesh.MaterialPasses.Count == 1 && w3dMesh.MaterialPasses[0].ShaderMaterialIds != null)
            {
                if (w3dMesh.MaterialPasses[0].ShaderMaterialIds.Items.Length > 1)
                {
                    throw new NotSupportedException();
                }
                var shaderMaterialID = w3dMesh.MaterialPasses[0].ShaderMaterialIds.Items[0];
                w3dShaderMaterial = w3dMesh.ShaderMaterials.Items[(int) shaderMaterialID];
                var effectName = w3dShaderMaterial.Header.TypeName.Replace(".fx", string.Empty);

                effect = contentManager.EffectLibrary.GetEffect(
                    effectName,
                    MeshVertex.VertexDescriptors);
            }
            else
            {
                w3dShaderMaterial = null;
                effect = contentManager.EffectLibrary.FixedFunction;
            }

            var vertexMaterials = CreateMaterials(w3dMesh);

            var shadingConfigurations = new FixedFunction.ShadingConfiguration[w3dMesh.Shaders.Items.Count];
            for (var i = 0; i < shadingConfigurations.Length; i++)
            {
                shadingConfigurations[i] = CreateShadingConfiguration(w3dMesh.Shaders.Items[i]);
            }

            var materialPasses = new ModelMeshMaterialPass[w3dMesh.MaterialPasses.Count];
            if (w3dShaderMaterial != null)
            {
                materialPasses[0] = CreateModelMeshMaterialPassShaderMaterial(
                    contentManager,
                    w3dMesh,
                    w3dMesh.MaterialPasses[0],
                    w3dShaderMaterial,
                    effect);
            }
            else
            {
                for (var i = 0; i < materialPasses.Length; i++)
                {
                    materialPasses[i] = CreateModelMeshMaterialPassFixedFunction(
                        contentManager,
                        w3dMesh,
                        w3dMesh.MaterialPasses[i],
                        vertexMaterials,
                        shadingConfigurations);
                }
            }

            var boundingBox = new BoundingBox(
                w3dMesh.Header.Min,
                w3dMesh.Header.Max);

            var cameraOriented = (w3dMesh.Header.Attributes & W3dMeshFlags.GeometryTypeMask) == W3dMeshFlags.GeometryTypeCameraOriented;

            var hasHouseColor = w3dMesh.Header.MeshName.StartsWith("HOUSECOLOR");

            return new ModelMesh(
                contentManager.GraphicsDevice,
                w3dMesh.Header.MeshName,
                MemoryMarshal.AsBytes(new ReadOnlySpan<MeshVertex.Basic>(CreateVertices(w3dMesh, w3dMesh.IsSkinned))),
                CreateIndices(w3dMesh, w3dShaderMaterial != null),
                materialPasses,
                w3dMesh.IsSkinned,
                boundingBox,
                w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.Hidden),
                cameraOriented,
                hasHouseColor);
        }

        private static FixedFunction.ShadingConfiguration CreateShadingConfiguration(W3dShader w3dShader)
        {
            return new FixedFunction.ShadingConfiguration
            {
                DiffuseLightingType = w3dShader.PrimaryGradient.ToDiffuseLightingType(),
                SpecularEnabled = w3dShader.SecondaryGradient == W3dShaderSecondaryGradient.Enable,
                TexturingEnabled = w3dShader.Texturing == W3dShaderTexturing.Enable,
                SecondaryTextureColorBlend = w3dShader.DetailColorFunc.ToSecondaryTextureBlend(),
                SecondaryTextureAlphaBlend = w3dShader.DetailAlphaFunc.ToSecondaryTextureBlend(),
                AlphaTest = w3dShader.AlphaTest == W3dShaderAlphaTest.Enable
            };
        }

        private static FixedFunction.VertexMaterial[] CreateMaterials(W3dMesh w3dMesh)
        {
            var vertexMaterials = new FixedFunction.VertexMaterial[w3dMesh.VertexMaterials.Items.Count];

            for (var i = 0; i < w3dMesh.VertexMaterials.Items.Count; i++)
            {
                var w3dMaterial = w3dMesh.VertexMaterials.Items[i];
                var w3dVertexMaterialInfo = w3dMaterial.Info;

                vertexMaterials[i] = w3dVertexMaterialInfo.ToVertexMaterial(w3dMaterial);
            }

            return vertexMaterials;
        }

        private static Texture CreateTexture(
            ContentManager contentManager,
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

            return CreateTexture(contentManager, w3dTexture.Name.Value);
        }

        private static Texture CreateTexture(
            ContentManager contentManager,
            string textureName)
        {
            var w3dTextureFilePath = Path.Combine("Art", "Textures", textureName);

            var texture = contentManager.Load<Texture>(w3dTextureFilePath, fallbackToPlaceholder: false);
            if (texture == null)
            {
                w3dTextureFilePath = Path.Combine("Art", "CompiledTextures", textureName.Substring(0, 2), textureName);
                texture = contentManager.Load<Texture>(w3dTextureFilePath);
            }

            return texture;
        }

        private static MeshVertex.Basic[] CreateVertices(
            W3dMesh w3dMesh,
            bool isSkinned)
        {
            var numVertices = (uint) w3dMesh.Vertices.Items.Length;
            var vertices = new MeshVertex.Basic[numVertices];

            for (var i = 0; i < numVertices; i++)
            {
                vertices[i] = new MeshVertex.Basic
                {
                    Position = w3dMesh.Vertices.Items[i],
                    Normal = w3dMesh.Normals.Items[i],
                    Tangent = w3dMesh.Tangents != null
                        ? w3dMesh.Tangents.Items[i]
                        : Vector3.Zero,
                    Binormal = w3dMesh.Bitangents != null
                        ? w3dMesh.Bitangents.Items[i]
                        : Vector3.Zero,
                    BoneIndex = isSkinned
                        ? w3dMesh.Influences.Items[i].BoneIndex
                        : 0u
                };
            }

            return vertices;
        }

        private static ushort[] CreateIndices(W3dMesh w3dMesh, bool usesShaderMaterial)
        {
            if (usesShaderMaterial)
            {
                // If using shader materials, we can use the actual shade indices from the mesh.
                return w3dMesh.ShadeIndices.Items.Select(x => (ushort) x).ToArray();
            }

            // Otherwise, we're doing some trickery to reduce the number of draw calls,
            // and this means we can't use mesh shade indices.

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

        private ModelMeshMaterialPass CreateModelMeshMaterialPassShaderMaterial(
            ContentManager contentManager,
            W3dMesh w3dMesh,
            W3dMaterialPass w3dMaterialPass,
            W3dShaderMaterial w3dShaderMaterial,
            Effect effect)
        {
            var texCoords = new MeshVertex.TexCoords[w3dMesh.Header.NumVertices];

            if (w3dMaterialPass.TexCoords != null)
            {
                for (var i = 0; i < texCoords.Length; i++)
                {
                    texCoords[i].UV0 = w3dMaterialPass.TexCoords.Items[i];
                }
            }

            var meshParts = new List<ModelMeshPart>();

            // TODO: Extract state properties from shader material.
            var rasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
            var depthState = DepthStencilStateDescription.DepthOnlyLessEqual;
            var blendState = BlendStateDescription.SingleDisabled;

            var material = new ShaderMaterial(contentManager, effect)
            {
                PipelineState = new EffectPipelineState(
                    rasterizerState,
                    depthState,
                    blendState,
                    RenderPipeline.GameOutputDescription)
            };

            var materialConstantsBuilder = new ShaderMaterialConstantsBuilder(effect);

            foreach (var w3dShaderProperty in w3dShaderMaterial.Properties)
            {
                switch (w3dShaderProperty.PropertyType)
                {
                    case W3dShaderMaterialPropertyType.Texture:
                        var texture = CreateTexture(contentManager, w3dShaderProperty.StringValue);
                        material.SetProperty(w3dShaderProperty.PropertyName, texture);
                        break;

                    case W3dShaderMaterialPropertyType.Bool:
                        materialConstantsBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Bool);
                        break;

                    case W3dShaderMaterialPropertyType.Float:
                        materialConstantsBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Float);
                        break;

                    case W3dShaderMaterialPropertyType.Vector2:
                        materialConstantsBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector2);
                        break;

                    case W3dShaderMaterialPropertyType.Vector3:
                        materialConstantsBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector3);
                        break;

                    case W3dShaderMaterialPropertyType.Vector4:
                        materialConstantsBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector4);
                        break;

                    case W3dShaderMaterialPropertyType.Int:
                        materialConstantsBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Int);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            material.SetMaterialConstants(AddDisposable(materialConstantsBuilder.CreateBuffer()));

            meshParts.Add(new ModelMeshPart(
                0,
                w3dMesh.Header.NumTris * 3,
                material,
                _meshDepthMaterialShader));

            return new ModelMeshMaterialPass(
                contentManager.GraphicsDevice,
                texCoords,
                meshParts);
        }

        // One ModelMeshMaterialPass for each W3D_CHUNK_MATERIAL_PASS
        private ModelMeshMaterialPass CreateModelMeshMaterialPassFixedFunction(
            ContentManager contentManager,
            W3dMesh w3dMesh,
            W3dMaterialPass w3dMaterialPass,
            FixedFunction.VertexMaterial[] vertexMaterials,
            FixedFunction.ShadingConfiguration[] shadingConfigurations)
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

            var texCoords = new MeshVertex.TexCoords[w3dMesh.Header.NumVertices];

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

            var meshParts = new List<ModelMeshPart>();

            // Optimisation for a fairly common case.
            if (w3dMaterialPass.VertexMaterialIds.Items.Length == 1
                && w3dMaterialPass.ShaderIds.Items.Length == 1
                && w3dMaterialPass.TextureStages.Count == 1
                && w3dMaterialPass.TextureStages[0].TextureIds.Items.Count == 1)
            {
                meshParts.Add(CreateModelMeshPart(
                    contentManager,
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
                            contentManager,
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
                        contentManager,
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

            return new ModelMeshMaterialPass(
                contentManager.GraphicsDevice,
                texCoords,
                meshParts);
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
                var hashCode = -493973629;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + VertexMaterialID.GetHashCode();
                hashCode = hashCode * -1521134295 + ShaderID.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<uint?>.Default.GetHashCode(TextureIndex0);
                hashCode = hashCode * -1521134295 + EqualityComparer<uint?>.Default.GetHashCode(TextureIndex1);
                return hashCode;
            }

            public static bool operator==(CombinedMaterialPermutation l, CombinedMaterialPermutation r)
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
            ContentManager contentManager,
            uint startIndex,
            uint indexCount,
            W3dMesh w3dMesh,
            FixedFunction.VertexMaterial[] vertexMaterials,
            FixedFunction.ShadingConfiguration[] shadingConfigurations,
            uint vertexMaterialID,
            uint shaderID,
            uint numTextureStages,
            uint? textureIndex0,
            uint? textureIndex1)
        {
            var w3dShader = w3dMesh.Shaders.Items[(int) shaderID];

            var rasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
            rasterizerState.CullMode = w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.TwoSided)
                ? FaceCullMode.None
                : FaceCullMode.Back;

            var depthState = DepthStencilStateDescription.DepthOnlyLessEqual;
            depthState.DepthWriteEnabled = w3dShader.DepthMask == W3dShaderDepthMask.WriteEnable;
            depthState.DepthComparison = w3dShader.DepthCompare.ToComparison();

            var blendState = new BlendStateDescription(
                RgbaFloat.White,
                new BlendAttachmentDescription(
                    (w3dShader.SrcBlend != W3dShaderSrcBlendFunc.One || w3dShader.DestBlend != W3dShaderDestBlendFunc.Zero),
                    w3dShader.SrcBlend.ToBlend(),
                    w3dShader.DestBlend.ToBlend(false),
                    BlendFunction.Add,
                    w3dShader.SrcBlend.ToBlend(),
                    w3dShader.DestBlend.ToBlend(true),
                    BlendFunction.Add));

            var effectMaterial = new FixedFunctionMaterial(contentManager, contentManager.EffectLibrary.FixedFunction)
            {
                PipelineState = new EffectPipelineState(
                    rasterizerState,
                    depthState,
                    blendState,
                    RenderPipeline.GameOutputDescription)
            };

            var materialConstantsBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                new FixedFunction.MaterialConstantsType
                {
                    Material = vertexMaterials[vertexMaterialID],
                    Shading = shadingConfigurations[shaderID],
                    NumTextureStages = (int)numTextureStages
                },
                BufferUsage.UniformBuffer));

            effectMaterial.SetMaterialConstants(materialConstantsBuffer);
            effectMaterial.SetTexture0(CreateTexture(contentManager, w3dMesh, textureIndex0));
            effectMaterial.SetTexture1(CreateTexture(contentManager, w3dMesh, textureIndex1));

            return new ModelMeshPart(
                startIndex,
                indexCount,
                effectMaterial,
                _meshDepthMaterialFixedFunction);
        }

        private static Animation CreateAnimation(W3dAnimation w3dAnimation)
        {
            var name = w3dAnimation.Header.HierarchyName + "." + w3dAnimation.Header.Name;
            var duration = TimeSpan.FromSeconds(w3dAnimation.Header.NumFrames / (double) w3dAnimation.Header.FrameRate);

            var channels = w3dAnimation.Channels
                .OfType<W3dAnimationChannel>()
                .Where(x => x.ChannelType != W3dAnimationChannelType.UnknownBfme) // Don't know what this channel means.
                .ToList();

            var bitChannels = w3dAnimation.Channels
                .OfType<W3dBitChannel>()
                .ToList();

            var clips = new AnimationClip[channels.Count + bitChannels.Count];

            for (var i = 0; i < channels.Count; i++)
            {
                clips[i] = CreateAnimationClip(w3dAnimation, channels[i]);
            }

            for (var i = 0; i < bitChannels.Count; i++)
            {
                clips[channels.Count + i] = CreateAnimationClip(w3dAnimation, bitChannels[i]);
            }

            return new Animation(
                name,
                duration,
                clips);
        }

        private static Animation CreateAnimation(W3dCompressedAnimation w3dAnimation)
        {
            var name = w3dAnimation.Header.HierarchyName + "." + w3dAnimation.Header.Name;
            var duration = TimeSpan.FromSeconds(w3dAnimation.Header.NumFrames / (double) w3dAnimation.Header.FrameRate);

            var timeCodedChannels = w3dAnimation.TimeCodedChannels
                .Where(x => x.ChannelType != W3dAnimationChannelType.UnknownBfme) // Don't know what this channel means.
                .ToList();

            var adaptiveDeltaChannels = w3dAnimation.AdaptiveDeltaChannels
               .Where(x => x.ChannelType != W3dAnimationChannelType.UnknownBfme) // Don't know what this channel means.
               .ToList();

            var motionChannels = w3dAnimation.MotionChannels
                .Where(x => x.ChannelType != W3dAnimationChannelType.UnknownBfme) // Don't know what this channel means.
                .ToList();

            var channelCount = timeCodedChannels.Count + adaptiveDeltaChannels.Count + motionChannels.Count;
            var clips = new AnimationClip[channelCount];

            for (var i = 0; i < timeCodedChannels.Count; i++)
            {
                clips[i] = CreateAnimationClip(w3dAnimation, timeCodedChannels[i]);
            }

            var timecodedAndAdaptive = timeCodedChannels.Count + adaptiveDeltaChannels.Count;

            for (var i = timeCodedChannels.Count; i < timecodedAndAdaptive; ++i)
            {
                clips[i] = CreateAnimationClip(w3dAnimation, adaptiveDeltaChannels[i - timeCodedChannels.Count]);
            }

            for (var i = timecodedAndAdaptive; i < channelCount; ++i)
            {
                clips[i] = CreateAnimationClip(w3dAnimation, motionChannels[i - timecodedAndAdaptive]);
            }

            return new Animation(
                name,
                duration,
                clips);
        }

        private static AnimationClip CreateAnimationClip(W3dAnimation w3dAnimation, W3dAnimationChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var data = w3dChannel.Data;
            var numKeyframes = data.GetLength(0);
            var keyframes = new Keyframe[numKeyframes];

            for (var i = 0; i < numKeyframes; i++)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.FirstFrame + i) / (double) w3dAnimation.Header.FrameRate);
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, data[i]);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static Keyframe CreateKeyframe(W3dAnimationChannelType channelType, TimeSpan time, in W3dAnimationChannelDatum datum)
        {
            return new Keyframe(time, CreateKeyframeValue(channelType, datum));
        }

        private static KeyframeValue CreateKeyframeValue(W3dAnimationChannelType channelType, in W3dAnimationChannelDatum datum)
        {
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    return new KeyframeValue { Quaternion = datum.Quaternion };

                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                    return new KeyframeValue { FloatValue = datum.FloatValue };

                default:
                    throw new NotImplementedException();
            }
        }

        private static AnimationClip CreateAnimationClip(W3dAnimation w3dAnimation, W3dBitChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var data = w3dChannel.Data;

            var numKeyframes = data.GetLength(0);

            var totalKeyframes = numKeyframes;
            if (w3dChannel.FirstFrame != 0)
            {
                totalKeyframes++;
            }
            if (w3dChannel.LastFrame != w3dAnimation.Header.NumFrames - 1)
            {
                totalKeyframes++;
            }

            var keyframes = new Keyframe[totalKeyframes];

            var keyframeIndex = 0;
            if (w3dChannel.FirstFrame != 0)
            {
                keyframes[keyframeIndex++] = new Keyframe(TimeSpan.Zero, new KeyframeValue { BoolValue = w3dChannel.DefaultValue });
            }

            for (var i = 0; i < numKeyframes; i++)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.FirstFrame + i) / (double) w3dAnimation.Header.FrameRate);

                switch (w3dChannel.ChannelType)
                {
                    case W3dBitChannelType.Visibility:
                        keyframes[keyframeIndex++] = new Keyframe(time, new KeyframeValue { BoolValue = data[i] });
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            if (w3dChannel.LastFrame != w3dAnimation.Header.NumFrames - 1)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.LastFrame + 1) / (double) w3dAnimation.Header.FrameRate);
                keyframes[keyframeIndex++] = new Keyframe(time, new KeyframeValue { BoolValue = w3dChannel.DefaultValue });
            }

            return new AnimationClip(AnimationClipType.Visibility, bone, keyframes);
        }

        private static AnimationClip CreateAnimationClip(W3dCompressedAnimation w3dAnimation, W3dTimeCodedAnimationChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var keyframes = new Keyframe[w3dChannel.NumTimeCodes];

            for (var i = 0; i < w3dChannel.NumTimeCodes; i++)
            {
                var timeCodedDatum = w3dChannel.Data[i];
                var time = TimeSpan.FromSeconds(timeCodedDatum.TimeCode / (double) w3dAnimation.Header.FrameRate);
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, timeCodedDatum.Value);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static AnimationClip CreateAnimationClip(W3dCompressedAnimation w3dAnimation, W3dAdaptiveDeltaAnimationChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var keyframes = new Keyframe[w3dChannel.NumTimeCodes];

            var decodedData = W3dAdaptiveDeltaCodec.Decode(
                w3dChannel.Data,
                w3dChannel.NumTimeCodes,
                w3dChannel.Scale);

            for (var i = 0; i < w3dChannel.NumTimeCodes; i++)
            {
                var time = TimeSpan.FromSeconds(i / (double) w3dAnimation.Header.FrameRate);
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, decodedData[i]);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static AnimationClip CreateAnimationClip(W3dCompressedAnimation w3dAnimation, W3dMotionChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var keyframes = new Keyframe[w3dChannel.NumTimeCodes];
            var i = 0;
            foreach (var keyframeWithValue in w3dChannel.Data.GetKeyframesWithValues(w3dChannel))
            {
                var time = TimeSpan.FromSeconds(keyframeWithValue.Keyframe / (double) w3dAnimation.Header.FrameRate);
                keyframes[i++] = CreateKeyframe(w3dChannel.ChannelType, time, keyframeWithValue.Datum);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }
    }
}

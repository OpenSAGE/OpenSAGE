﻿using System;
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
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Content
{
    internal sealed class ModelLoader : ContentLoader<Model>
    {
        protected override Model LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            var w3dFile = W3dFile.FromFileSystemEntry(entry);

            var w3dHierarchy = w3dFile.Hierarchy;
            if (w3dFile.HLod != null && w3dHierarchy == null)
            {
                // Load referenced hierarchy.
                var hierarchyFileName = w3dFile.HLod.Header.HierarchyName + ".W3D";
                var hierarchyFilePath = Path.Combine(Path.GetDirectoryName(w3dFile.FilePath), hierarchyFileName);
                var hierarchyFileEntry = contentManager.FileSystem.GetFile(hierarchyFilePath);
                var hierarchyFile = W3dFile.FromFileSystemEntry(hierarchyFileEntry);
                w3dHierarchy = hierarchyFile.Hierarchy;
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
                bones = new ModelBone[w3dHierarchy.Pivots.Length];

                for (var i = 0; i < w3dHierarchy.Pivots.Length; i++)
                {
                    var pivot = w3dHierarchy.Pivots[i];

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

            var meshes = new ModelMesh[w3dFile.Meshes.Count];

            for (var i = 0; i < w3dFile.Meshes.Count; i++)
            {
                var w3dMesh = w3dFile.Meshes[i];

                ModelBone bone;
                if (w3dFile.HLod != null)
                {
                    var hlodSubObject = w3dFile.HLod.Lods[0].SubObjects.Single(x => x.Name == w3dMesh.Header.ContainerName + "." + w3dMesh.Header.MeshName);
                    bone = bones[(int) hlodSubObject.BoneIndex];
                }
                else
                {
                    bone = bones[0];
                }

                meshes[i] = CreateModelMesh(
                    contentManager,
                    w3dMesh,
                    bone,
                    bones.Length);

                //var meshBoundingSphere = mesh.BoundingSphere.Transform(bone.Transform);

                //boundingSphere = (i == 0)
                //    ? meshBoundingSphere
                //    : BoundingSphere.CreateMerged(boundingSphere, meshBoundingSphere);
            }

            var animations = new Animation[w3dFile.Animations.Count + w3dFile.CompressedAnimations.Count];
            for (var i = 0; i < w3dFile.Animations.Count; i++)
            {
                animations[i] = CreateAnimation(w3dFile.Animations[i]);
            }
            for (var i = 0; i < w3dFile.CompressedAnimations.Count; i++)
            {
                animations[w3dFile.Animations.Count + i] = CreateAnimation(w3dFile.CompressedAnimations[i]);
            }

            return new Model(
                bones,
                meshes,
                animations);
        }

        private ModelMesh CreateModelMesh(
            ContentManager contentManager,
            W3dMesh w3dMesh,
            ModelBone parentBone,
            int numBones)
        {
            W3dShaderMaterial w3dShaderMaterial;
            Effect effect = null;
            if (w3dMesh.MaterialPasses.Length == 1 && w3dMesh.MaterialPasses[0].ShaderMaterialId != null)
            {
                var shaderMaterialID = w3dMesh.MaterialPasses[0].ShaderMaterialId.Value;
                w3dShaderMaterial = w3dMesh.ShaderMaterials.Materials[(int) shaderMaterialID];
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

            var shadingConfigurations = new FixedFunction.ShadingConfiguration[w3dMesh.Shaders.Length];
            for (var i = 0; i < shadingConfigurations.Length; i++)
            {
                shadingConfigurations[i] = CreateShadingConfiguration(w3dMesh.Shaders[i]);
            }

            var materialPasses = new ModelMeshMaterialPass[w3dMesh.MaterialPasses.Length];
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

            return new ModelMesh(
                contentManager.GraphicsDevice,
                w3dMesh.Header.MeshName,
                MemoryMarshal.AsBytes(new ReadOnlySpan<MeshVertex.Basic>(CreateVertices(w3dMesh, w3dMesh.IsSkinned))),
                CreateIndices(w3dMesh),
                effect,
                materialPasses,
                w3dMesh.IsSkinned,
                parentBone,
                (uint) numBones,
                boundingBox,
                w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.Hidden),
                cameraOriented);
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
            var vertexMaterials = new FixedFunction.VertexMaterial[w3dMesh.Materials.Length];

            for (var i = 0; i < w3dMesh.Materials.Length; i++)
            {
                var w3dMaterial = w3dMesh.Materials[i];
                var w3dVertexMaterial = w3dMaterial.VertexMaterialInfo;

                vertexMaterials[i] = w3dVertexMaterial.ToVertexMaterial(w3dMaterial);
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

            var w3dTexture = w3dMesh.Textures[(int) textureIndex];

            if (w3dTexture.TextureInfo != null && w3dTexture.TextureInfo.FrameCount != 1)
            {
                throw new NotImplementedException();
            }

            return CreateTexture(contentManager, w3dTexture.Name);
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
            var numVertices = (uint) w3dMesh.Vertices.Length;
            var vertices = new MeshVertex.Basic[numVertices];

            for (var i = 0; i < numVertices; i++)
            {
                vertices[i] = new MeshVertex.Basic
                {
                    Position = w3dMesh.Vertices[i],
                    Normal = w3dMesh.Normals[i],
                    Tangent = w3dMesh.Tangents != null
                        ? w3dMesh.Tangents[i]
                        : Vector3.Zero,
                    Binormal = w3dMesh.Bitangents != null
                        ? w3dMesh.Bitangents[i]
                        : Vector3.Zero,
                    BoneIndex = isSkinned
                        ? w3dMesh.Influences[i].BoneIndex
                        : 0u
                };
            }

            return vertices;
        }

        private static ushort[] CreateIndices(W3dMesh w3dMesh)
        {
            var triangles = w3dMesh.Triangles.AsSpan();
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
                    texCoords[i].UV0 = w3dMaterialPass.TexCoords[i];
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

            var depthMaterial = new MeshDepthMaterial(contentManager, contentManager.EffectLibrary.MeshDepth);

            meshParts.Add(new ModelMeshPart(
                0,
                w3dMesh.Header.NumTris * 3,
                material,
                depthMaterial));

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
                        texCoords[i].UV0 = textureStage0.TexCoords[i];
                    }

                    if (hasTextureStage1)
                    {
                        texCoords[i].UV1 = textureStage1.TexCoords[i];
                    }
                }
            }

            var meshParts = new List<ModelMeshPart>();

            // Optimisation for a fairly common case.
            if (w3dMaterialPass.VertexMaterialIds.Length == 1
                && w3dMaterialPass.ShaderIds.Length == 1
                && w3dMaterialPass.TextureStages.Count == 1
                && w3dMaterialPass.TextureStages[0].TextureIds.Length == 1)
            {
                meshParts.Add(CreateModelMeshPart(
                    contentManager,
                    0, 
                    w3dMesh.Header.NumTris * 3,
                    w3dMesh,
                    vertexMaterials,
                    shadingConfigurations,
                    w3dMaterialPass.VertexMaterialIds[0],
                    w3dMaterialPass.ShaderIds[0],
                    numTextureStages,
                    w3dMaterialPass.TextureStages[0].TextureIds[0],
                    null));
            }
            else
            {
                // Expand ShaderIds and TextureIds, if they have a single entry
                // (which means same ID for all faces)

                IEnumerable<uint?> getExpandedTextureIds(uint?[] ids)
                {
                    if (ids == null)
                    {
                        for (var i = 0; i < w3dMesh.Header.NumTris; i++)
                        {
                            yield return null;
                        }
                    }
                    else if (ids.Length == 1)
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
                    var ids = w3dMaterialPass.ShaderIds;
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
                    var ids = w3dMaterialPass.VertexMaterialIds;
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
                            var triangle = w3dMesh.Triangles[i];
                            var materialID0 = ids[triangle.VIndex0];
                            var materialID1 = ids[triangle.VIndex1];
                            var materialID2 = ids[triangle.VIndex2];
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
                    .Zip(getExpandedTextureIds(textureStage0?.TextureIds), (x, y) => new { x.VertexMaterialID, x.ShaderID, TextureIndex0 = y })
                    .Zip(getExpandedTextureIds(textureStage1?.TextureIds), (x, y) => new CombinedMaterialPermutation { VertexMaterialID = x.VertexMaterialID, ShaderID = x.ShaderID, TextureIndex0 = x.TextureIndex0, TextureIndex1 = y });

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
            var w3dShader = w3dMesh.Shaders[shaderID];

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

            var depthMaterial = new MeshDepthMaterial(contentManager, contentManager.EffectLibrary.MeshDepth);

            return new ModelMeshPart(
                startIndex,
                indexCount,
                effectMaterial,
                depthMaterial);
        }

        private static Animation CreateAnimation(W3dAnimation w3dAnimation)
        {
            var name = w3dAnimation.Header.Name;
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
            var name = w3dAnimation.Header.Name;
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
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, ref data[i]);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static Keyframe CreateKeyframe(W3dAnimationChannelType channelType, TimeSpan time, ref W3dAnimationChannelDatum datum)
        {
            return new Keyframe(time, CreateKeyframeValue(channelType, ref datum));
        }

        private static KeyframeValue CreateKeyframeValue(W3dAnimationChannelType channelType, ref W3dAnimationChannelDatum datum)
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
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, ref timeCodedDatum.Value);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static AnimationClip CreateAnimationClip(W3dCompressedAnimation w3dAnimation, W3dAdaptiveDeltaAnimationChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var keyframes = new Keyframe[w3dChannel.NumTimeCodes];

            for (var i = 0; i < w3dChannel.NumTimeCodes; i++)
            {
                var time = TimeSpan.FromSeconds(i / (double) w3dAnimation.Header.FrameRate);
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, ref w3dChannel.Data[i]);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static AnimationClip CreateAnimationClip(W3dCompressedAnimation w3dAnimation, W3dMotionChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var keyframes = new Keyframe[w3dChannel.NumTimeCodes];

            for (var i = 0; i < w3dChannel.Data.TimeCodes.Length; i++)
            {
                var time = TimeSpan.FromSeconds(w3dChannel.Data.TimeCodes[i] / (double) w3dAnimation.Header.FrameRate);
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, ref w3dChannel.Data.Values[i]);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }
    }
}

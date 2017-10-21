using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using LLGfx;
using LLGfx.Effects;
using OpenSage.Content.Util;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;

namespace OpenSage.Content
{
    internal sealed class ModelLoader : ContentLoader<Model>
    {
        protected override Model LoadEntry(FileSystemEntry entry, ContentManager contentManager, ResourceUploadBatch uploadBatch)
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
                uploadBatch,
                w3dFile,
                w3dHierarchy);
        }

        private static Model CreateModel(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dFile w3dFile,
            W3dHierarchyDef w3dHierarchy)
        {
            ModelBone[] bones;
            if (w3dHierarchy != null)
            {
                if (w3dHierarchy.Pivots.Length > ModelMesh.MaxBones)
                {
                    throw new NotSupportedException();
                }

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
                    uploadBatch,
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

        private static ModelMesh CreateModelMesh(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh,
            ModelBone parentBone,
            int numBones)
        {
            var materialPasses = new ModelMeshMaterialPass[w3dMesh.MaterialPasses.Length];
            for (var i = 0; i < materialPasses.Length; i++)
            {
                materialPasses[i] = CreateModelMeshMaterialPass(
                    contentManager.GraphicsDevice,
                    uploadBatch,
                    w3dMesh,
                    w3dMesh.MaterialPasses[i]);
            }

            var boundingBox = new BoundingBox(
                w3dMesh.Header.Min,
                w3dMesh.Header.Max);

            var isSkinned = w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.GeometryTypeSkin);

            return new ModelMesh(
                contentManager.GraphicsDevice,
                uploadBatch,
                w3dMesh.Header.MeshName,
                CreateVertices(w3dMesh, isSkinned),
                CreateIndices(w3dMesh),
                CreateMaterials(w3dMesh),
                CreateTextures(contentManager, uploadBatch, w3dMesh),
                materialPasses,
                isSkinned,
                parentBone,
                (uint) numBones,
                boundingBox);
        }

        private static VertexMaterial[] CreateMaterials(W3dMesh w3dMesh)
        {
            var vertexMaterials = new VertexMaterial[w3dMesh.Materials.Length];

            for (var i = 0; i < w3dMesh.Materials.Length; i++)
            {
                var w3dMaterial = w3dMesh.Materials[i];
                var w3dVertexMaterial = w3dMaterial.VertexMaterialInfo;

                vertexMaterials[i] = w3dVertexMaterial.ToVertexMaterial(w3dMaterial);
            }

            return vertexMaterials;
        }

        private static Texture[] CreateTextures(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh)
        {
            var numTextures = w3dMesh.Textures.Count;
            var textures = new Texture[numTextures];

            for (var i = 0; i < numTextures; i++)
            {
                var w3dTexture = w3dMesh.Textures[i];

                if (w3dTexture.TextureInfo != null && w3dTexture.TextureInfo.FrameCount != 1)
                {
                    throw new NotImplementedException();
                }

                var w3dTextureFilePath = Path.Combine("Art", "Textures", w3dTexture.Name);
                textures[i] = contentManager.Load<Texture>(w3dTextureFilePath, uploadBatch);
            }

            return textures;
        }

        private static MeshVertex[] CreateVertices(
            W3dMesh w3dMesh,
            bool isSkinned)
        {
            var numVertices = (uint) w3dMesh.Vertices.Length;
            var vertices = new MeshVertex[numVertices];

            for (var i = 0; i < numVertices; i++)
            {
                vertices[i] = new MeshVertex
                {
                    Position = w3dMesh.Vertices[i],
                    Normal = w3dMesh.Normals[i],
                    BoneIndex = isSkinned
                        ? w3dMesh.Influences[i].BoneIndex
                        : 0u
                };
            }

            return vertices;
        }

        private static ushort[] CreateIndices(W3dMesh w3dMesh)
        {
            var numIndices = (uint) w3dMesh.Triangles.Length * 3;
            var indices = new ushort[numIndices];

            var indexIndex = 0;
            foreach (var triangle in w3dMesh.Triangles)
            {
                indices[indexIndex++] = (ushort) triangle.VIndex0;
                indices[indexIndex++] = (ushort) triangle.VIndex1;
                indices[indexIndex++] = (ushort) triangle.VIndex2;
            }

            return indices;
        }

        // One ModelMeshMaterialPass for each W3D_CHUNK_MATERIAL_PASS
        private static ModelMeshMaterialPass CreateModelMeshMaterialPass(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh,
            W3dMaterialPass w3dMaterialPass)
        {
            var hasTextureStage0 = w3dMaterialPass.TextureStages.Count > 0;
            var textureStage0 = hasTextureStage0
                ? w3dMaterialPass.TextureStages[0]
                : null;

            var hasTextureStage1 = w3dMaterialPass.TextureStages.Count > 1;
            var textureStage1 = hasTextureStage1
                ? w3dMaterialPass.TextureStages[1]
                : null;

            var numTextureStages = hasTextureStage0 && hasTextureStage1
                ? 2u
                : hasTextureStage0 ? 1u : 0u;

            var texCoords = new MeshTexCoords[w3dMesh.Header.NumVertices];

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

            var textureIndices = new MeshTextureIndex[w3dMesh.Header.NumTris];

            if (hasTextureStage0)
            {
                if (textureStage0.TextureIds.Length == 1)
                {
                    var textureID = textureStage0.TextureIds[0];
                    for (var i = 0; i < textureIndices.Length; i++)
                    {
                        textureIndices[i].IndexStage0 = textureID;
                    }
                }
                else
                {
                    for (var i = 0; i < textureIndices.Length; i++)
                    {
                        textureIndices[i].IndexStage0 = textureStage0.TextureIds[i];
                    }
                }
            }

            if (hasTextureStage1)
            {
                if (textureStage1.TextureIds.Length == 1)
                {
                    var textureID = textureStage1.TextureIds[0];
                    for (var i = 0; i < textureIndices.Length; i++)
                    {
                        textureIndices[i].IndexStage1 = textureID;
                    }
                }
                else
                {
                    for (var i = 0; i < textureIndices.Length; i++)
                    {
                        textureIndices[i].IndexStage1 = textureStage1.TextureIds[i];
                    }
                }
            }

            var materialIndices = w3dMaterialPass.VertexMaterialIds;
            if (materialIndices.Length == 1)
            {
                var materialID = materialIndices[0];
                materialIndices = new uint[w3dMesh.Header.NumVertices];
                for (var i = 0; i < w3dMesh.Header.NumVertices; i++)
                {
                    materialIndices[i] = materialID;
                }
            }

            var meshParts = new List<ModelMeshPart>();

            if (w3dMaterialPass.ShaderIds.Length == 1)
            {
                meshParts.Add(CreateModelMeshPart(
                    0, 
                    w3dMesh.Header.NumTris * 3,
                    w3dMesh,
                    w3dMesh.Shaders[w3dMaterialPass.ShaderIds[0]]));
            }
            else
            {
                var shaderID = w3dMaterialPass.ShaderIds[0];
                var startIndex = 0u;
                var indexCount = 0u;
                for (var i = 0; i < w3dMaterialPass.ShaderIds.Length; i++)
                {
                    var newShaderID = w3dMaterialPass.ShaderIds[i];

                    if (shaderID != newShaderID)
                    {
                        meshParts.Add(CreateModelMeshPart(
                            startIndex,
                            indexCount,
                            w3dMesh,
                            w3dMesh.Shaders[shaderID]));

                        startIndex = (uint) (i * 3);
                        indexCount = 0;
                    }

                    shaderID = newShaderID;

                    indexCount += 3;
                }

                if (indexCount > 0)
                {
                    meshParts.Add(CreateModelMeshPart(
                        startIndex,
                        indexCount,
                        w3dMesh,
                        w3dMesh.Shaders[shaderID]));
                }
            }

            return new ModelMeshMaterialPass(
                graphicsDevice,
                uploadBatch,
                numTextureStages,
                texCoords,
                textureIndices,
                materialIndices,
                meshParts);
        }

        // One ModelMeshPart for each unique shader in a W3D_CHUNK_MATERIAL_PASS.
        private static ModelMeshPart CreateModelMeshPart(
            uint startIndex,
            uint indexCount,
            W3dMesh w3dMesh,
            W3dShader w3dShader)
        {
            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.CullMode = w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.TwoSided)
                ? CullMode.None
                : CullMode.CullBack;

            var depthState = DepthStencilStateDescription.Default;
            depthState.IsDepthEnabled = true;
            depthState.IsDepthWriteEnabled = w3dShader.DepthMask == W3dShaderDepthMask.WriteEnable;
            // TODO: DepthCompare

            var blendState = BlendStateDescription.Opaque;
            blendState.Enabled = w3dShader.SrcBlend != W3dShaderSrcBlendFunc.One
                || w3dShader.DestBlend != W3dShaderDestBlendFunc.Zero;
            blendState.SourceBlend = w3dShader.SrcBlend.ToBlend();
            blendState.DestinationBlend = w3dShader.DestBlend.ToBlend();

            var pipelineStateHandle = new EffectPipelineState(
                rasterizerState,
                depthState,
                blendState)
                .GetHandle();

            return new ModelMeshPart(
                startIndex,
                indexCount,
                w3dShader.AlphaTest == W3dShaderAlphaTest.Enable,
                w3dShader.Texturing == W3dShaderTexturing.Enable,
                pipelineStateHandle);
        }

        private static Animation CreateAnimation(W3dAnimation w3dAnimation)
        {
            var name = w3dAnimation.Header.Name;
            var duration = TimeSpan.FromSeconds(w3dAnimation.Header.NumFrames / (double) w3dAnimation.Header.FrameRate);

            var clips = new AnimationClip[w3dAnimation.Channels.Count + w3dAnimation.BitChannels.Count];

            for (var i = 0; i < w3dAnimation.Channels.Count; i++)
            {
                clips[i] = CreateAnimationClip(w3dAnimation, w3dAnimation.Channels[i]);
            }

            for (var i = 0; i < w3dAnimation.BitChannels.Count; i++)
            {
                clips[w3dAnimation.Channels.Count + i] = CreateAnimationClip(w3dAnimation, w3dAnimation.BitChannels[i]);
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

            var clips = new AnimationClip[w3dAnimation.TimeCodedChannels.Count];

            for (var i = 0; i < w3dAnimation.TimeCodedChannels.Count; i++)
            {
                clips[i] = CreateAnimationClip(w3dAnimation, w3dAnimation.TimeCodedChannels[i]);
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

            return new AnimationClip(bone, keyframes);
        }

        private static Keyframe CreateKeyframe(W3dAnimationChannelType channelType, TimeSpan time, ref W3dAnimationChannelDatum datum)
        {
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    return new QuaternionKeyframe(time, datum.Quaternion);

                case W3dAnimationChannelType.TranslationX:
                    return new TranslationXKeyframe(time, datum.FloatValue);

                case W3dAnimationChannelType.TranslationY:
                    return new TranslationYKeyframe(time, datum.FloatValue);

                case W3dAnimationChannelType.TranslationZ:
                    return new TranslationZKeyframe(time, datum.FloatValue);

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
                keyframes[keyframeIndex++] = new VisibilityKeyframe(TimeSpan.Zero, w3dChannel.DefaultValue);
            }

            for (var i = 0; i < numKeyframes; i++)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.FirstFrame + i) / (double) w3dAnimation.Header.FrameRate);

                switch (w3dChannel.ChannelType)
                {
                    case W3dBitChannelType.Visibility:
                        keyframes[keyframeIndex++] = new VisibilityKeyframe(time, data[i]);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            if (w3dChannel.LastFrame != w3dAnimation.Header.NumFrames - 1)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.LastFrame + 1) / (double) w3dAnimation.Header.FrameRate);
                keyframes[keyframeIndex++] = new VisibilityKeyframe(time, w3dChannel.DefaultValue);
            }

            return new AnimationClip(bone, keyframes);
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

            return new AnimationClip(bone, keyframes);
        }
    }
}

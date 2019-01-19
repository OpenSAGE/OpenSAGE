using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    /// <summary>
    /// A mesh is composed of the following hierarchy:
    /// 
    /// - Mesh: Vertices, Normals, Indices, Materials.
    ///   - MeshMaterialPasses[]: per-vertex TexCoords, 
    ///                           per-vertex Material indices, 
    ///                           per-triangle Texture indices.
    ///     - MeshParts[]: One for each unique PipelineState in a material pass.
    ///                    StartIndex, IndexCount, PipelineState, AlphaTest, Texturing
    /// </summary>
    public sealed class ModelMesh : DisposableBase
    {
        private readonly ShaderSet _shaderSet;
        private readonly ShaderSet _depthShaderSet;

        private readonly Pipeline _depthPipeline;

        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;

        private readonly ResourceSet _meshConstantsResourceSet;

        private readonly ResourceSet _samplerResourceSet;

        internal readonly BeforeRenderDelegate[] BeforeRenderDelegates;
        internal readonly BeforeRenderDelegate[] BeforeRenderDelegatesDepth;

        public readonly string Name;

        public readonly BoundingBox BoundingBox;

        public readonly List<ModelMeshPart> MeshParts;

        public readonly bool Skinned;

        public readonly bool Hidden;
        public readonly bool CameraOriented;

        internal ModelMesh(
            GraphicsDevice graphicsDevice,
            ShaderResourceManager shaderResources,
            string name,
            ShaderSet shaderSet,
            Pipeline depthPipeline,
            ReadOnlySpan<byte> vertexData,
            ushort[] indices,
            List<ModelMeshPart> meshParts,
            bool isSkinned,
            in BoundingBox boundingBox,
            bool hidden,
            bool cameraOriented,
            bool hasHouseColor)
        {
            Name = name;

            _shaderSet = shaderSet;
            _depthShaderSet = shaderResources.MeshDepth.ShaderSet;

            _depthPipeline = depthPipeline;

            BoundingBox = boundingBox;

            Skinned = isSkinned;

            Hidden = hidden;
            CameraOriented = cameraOriented;

            _vertexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(vertexData, BufferUsage.VertexBuffer));

            _indexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                indices,
                BufferUsage.IndexBuffer));

            _meshConstantsResourceSet = shaderResources.Mesh.GetCachedMeshResourceSet(
                isSkinned,
                hasHouseColor);

            _samplerResourceSet = shaderResources.Mesh.SamplerResourceSet;

            MeshParts = meshParts;

            BeforeRenderDelegates = new BeforeRenderDelegate[meshParts.Count];
            BeforeRenderDelegatesDepth = new BeforeRenderDelegate[meshParts.Count];

            for (var i = 0; i < BeforeRenderDelegates.Length; i++)
            {
                var meshPart = meshParts[i];

                BeforeRenderDelegates[i] = (cl, context) =>
                {
                    cl.SetGraphicsResourceSet(4, _meshConstantsResourceSet);
                    cl.SetGraphicsResourceSet(5, meshPart.MaterialResourceSet);
                    cl.SetGraphicsResourceSet(6, _samplerResourceSet);

                    cl.SetVertexBuffer(0, _vertexBuffer);

                    if (meshPart.TexCoordVertexBuffer != null)
                    {
                        cl.SetVertexBuffer(1, meshPart.TexCoordVertexBuffer);
                    }
                };

                BeforeRenderDelegatesDepth[i] = (cl, context) =>
                {
                    cl.SetGraphicsResourceSet(1, _meshConstantsResourceSet);

                    cl.SetVertexBuffer(0, _vertexBuffer);

                    if (meshPart.TexCoordVertexBuffer != null)
                    {
                        cl.SetVertexBuffer(1, meshPart.TexCoordVertexBuffer);
                    }
                };
            }
        }

        internal void BuildRenderList(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            BeforeRenderDelegate[] beforeRender,
            BeforeRenderDelegate[] beforeRenderDepth,
            ModelBone parentBone,
            in Matrix4x4 modelTransform,
            bool castsShadow,
            Player owner)
        {
            var meshWorldMatrix = Skinned
                ? modelTransform
                : modelInstance.AbsoluteBoneTransforms[parentBone.Index];

            BuildRenderListWithWorldMatrix(
                renderList,
                camera,
                modelInstance,
                beforeRender,
                beforeRenderDepth,
                parentBone,
                meshWorldMatrix,
                castsShadow,
                owner);
        }

        internal void BuildRenderListWithWorldMatrix(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            BeforeRenderDelegate[] beforeRender,
            BeforeRenderDelegate[] beforeRenderDepth,
            ModelBone parentBone,
            in Matrix4x4 meshWorldMatrix,
            bool castsShadow,
            Player owner = null)
        {
            if (Hidden)
            {
                return;
            }

            if (!modelInstance.BoneVisibilities[parentBone.Index])
            {
                return;
            }

            Matrix4x4 world;
            if (CameraOriented)
            {
                // TODO: I don't think this is correct yet.

                var localToWorldMatrix = meshWorldMatrix;

                var viewInverse = Matrix4x4Utility.Invert(camera.View);
                var cameraPosition = viewInverse.Translation;

                var toCamera = Vector3.Normalize(Vector3.TransformNormal(
                    cameraPosition - meshWorldMatrix.Translation,
                    Matrix4x4Utility.Invert(meshWorldMatrix)));

                toCamera.Z = 0;

                var cameraOrientedRotation = Matrix4x4.CreateFromQuaternion(
                    QuaternionUtility.CreateRotation(
                        Vector3.UnitX,
                        toCamera));

                world = cameraOrientedRotation * localToWorldMatrix;
            }
            else
            {
                world = meshWorldMatrix;
            }

            var meshBoundingBox = BoundingBox.Transform(BoundingBox, world); // TODO: Not right for skinned meshes

            for (var i = 0; i < MeshParts.Count; i++)
            {
                var meshPart = MeshParts[i];

                var blendEnabled = meshPart.BlendEnabled;

                // Depth pass

                // TODO: With more work, we could draw shadows for translucent and alpha-tested materials.
                if (!blendEnabled && castsShadow)
                {
                    renderList.Shadow.RenderItems.Add(new RenderItem(
                       _depthShaderSet,
                       _depthPipeline,
                       meshBoundingBox,
                       world,
                       meshPart.StartIndex,
                       meshPart.IndexCount,
                       _indexBuffer,
                       beforeRenderDepth[i]));
                }

                // Standard pass

                var renderQueue = blendEnabled
                    ? renderList.Transparent
                    : renderList.Opaque;

                renderQueue.RenderItems.Add(new RenderItem(
                    _shaderSet,
                    meshPart.Pipeline,
                    meshBoundingBox,
                    world,
                    meshPart.StartIndex,
                    meshPart.IndexCount,
                    _indexBuffer,
                    beforeRender[i],
                    owner?.Color));
            }
        }
    }
}

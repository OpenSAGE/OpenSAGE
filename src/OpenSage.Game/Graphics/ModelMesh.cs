using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
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
    public sealed partial class ModelMesh : BaseAsset
    {
        private readonly ShaderSet _shaderSet;
        private ShaderSet _depthShaderSet;

        private readonly Pipeline _depthPipeline;

        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;

        private readonly ResourceSet _meshConstantsResourceSet;

        private ResourceSet _samplerResourceSet;

        internal BeforeRenderDelegate[] BeforeRenderDelegates { get; private set; }
        internal BeforeRenderDelegate[] BeforeRenderDelegatesDepth { get; private set; }

        public readonly BoundingBox BoundingBox;

        public readonly List<ModelMeshPart> MeshParts;

        public readonly bool Skinned;

        public readonly bool Hidden;
        public readonly bool CameraOriented;

        private void PostInitialize(AssetLoadContext loadContext)
        {
            _samplerResourceSet = loadContext.ShaderResources.Mesh.SamplerResourceSet;
            _depthShaderSet = loadContext.ShaderResources.MeshDepth.ShaderSet;

            BeforeRenderDelegates = new BeforeRenderDelegate[MeshParts.Count];
            BeforeRenderDelegatesDepth = new BeforeRenderDelegate[MeshParts.Count];

            for (var i = 0; i < BeforeRenderDelegates.Length; i++)
            {
                var meshPart = MeshParts[i];

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
            MeshShaderResources.RenderItemConstantsPS? renderItemConstantsPS)
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
                renderItemConstantsPS);
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
            MeshShaderResources.RenderItemConstantsPS? renderItemConstantsPS = null)
        {
            if (Hidden || !modelInstance.BoneFrameVisibilities[parentBone.Index])
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

                var forceBlendEnabled = renderItemConstantsPS != null && renderItemConstantsPS.Value.Opacity < 1.0f;
                var blendEnabled = meshPart.BlendEnabled || forceBlendEnabled;

                // Depth pass

                // TODO: With more work, we could draw shadows for translucent and alpha-tested materials.
                if (!blendEnabled && castsShadow)
                {
                    renderList.Shadow.RenderItems.Add(new RenderItem(
                        Name,
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
                    Name,
                    _shaderSet,
                    forceBlendEnabled ? meshPart.PipelineBlend : meshPart.Pipeline,
                    meshBoundingBox,
                    world,
                    meshPart.StartIndex,
                    meshPart.IndexCount,
                    _indexBuffer,
                    beforeRender[i],
                    renderItemConstantsPS));
            }
        }
    }
}

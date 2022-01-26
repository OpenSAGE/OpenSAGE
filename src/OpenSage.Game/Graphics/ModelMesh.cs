using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Rendering;
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
    public sealed partial class ModelMesh : ModelRenderObject
    {
        internal readonly DeviceBuffer VertexBuffer;
        private readonly DeviceBuffer _indexBuffer;

        internal readonly ConstantBuffer<MeshShaderResources.MeshConstants> MeshConstantsBuffer;

        private readonly AxisAlignedBoundingBox _boundingBox;

        public override ref readonly AxisAlignedBoundingBox BoundingBox => ref _boundingBox;

        public readonly List<ModelMeshPart> MeshParts;

        public readonly bool Skinned;

        public override bool Hidden { get; }
        public readonly bool CameraOriented;

        internal override void BuildRenderList(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            ModelMeshInstance modelMeshInstance,
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
                modelMeshInstance,
                parentBone,
                meshWorldMatrix,
                castsShadow,
                renderItemConstantsPS);
        }

        internal override void BuildRenderListWithWorldMatrix(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            ModelMeshInstance modelMeshInstance,
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

            var meshBoundingBox = AxisAlignedBoundingBox.Transform(BoundingBox, world); // TODO: Not right for skinned meshes

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
                        modelMeshInstance.MeshPartInstances[i].ModelMeshPart.Material.ShadowPass,
                        meshBoundingBox,
                        world,
                        meshPart.StartIndex,
                        meshPart.IndexCount,
                        _indexBuffer,
                        modelMeshInstance.MeshPartInstances[i].BeforeRenderCallbackDepth));
                }

                // Standard pass

                var renderQueue = blendEnabled
                    ? renderList.Transparent
                    : renderList.Opaque;

                renderQueue.RenderItems.Add(new RenderItem(
                    FullName,
                    forceBlendEnabled
                        ? modelMeshInstance.MeshPartInstances[i].ModelMeshPart.MaterialBlend.ForwardPass
                        : modelMeshInstance.MeshPartInstances[i].ModelMeshPart.Material.ForwardPass,
                    meshBoundingBox,
                    world,
                    meshPart.StartIndex,
                    meshPart.IndexCount,
                    _indexBuffer,
                    modelMeshInstance.MeshPartInstances[i].BeforeRenderCallback));
            }
        }
    }
}

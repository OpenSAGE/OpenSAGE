using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
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
        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;

        private readonly ConstantBuffer<MeshMaterial.MeshConstants> _meshConstantsBuffer;

        public string Name { get; }

        public BoundingBox BoundingBox { get; }

        public ModelMeshMaterialPass[] MaterialPasses { get; }

        public bool Skinned { get; }

        public bool Hidden { get; }
        public bool CameraOriented { get; }

        internal ModelMesh(
            GraphicsDevice graphicsDevice,
            string name,
            ReadOnlySpan<byte> vertexData,
            ushort[] indices,
            ModelMeshMaterialPass[] materialPasses,
            bool isSkinned,
            BoundingBox boundingBox,
            bool hidden,
            bool cameraOriented,
            bool hasHouseColor)
        {
            Name = name;

            BoundingBox = boundingBox;

            Skinned = isSkinned;

            Hidden = hidden;
            CameraOriented = cameraOriented;

            _vertexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(vertexData, BufferUsage.VertexBuffer));

            _indexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                indices,
                BufferUsage.IndexBuffer));

            var commandEncoder = graphicsDevice.ResourceFactory.CreateCommandList();

            commandEncoder.Begin();

            _meshConstantsBuffer = AddDisposable(new ConstantBuffer<MeshMaterial.MeshConstants>(graphicsDevice));
            _meshConstantsBuffer.Value.SkinningEnabled = isSkinned;
            _meshConstantsBuffer.Value.HasHouseColor = hasHouseColor;
            _meshConstantsBuffer.Update(commandEncoder);

            commandEncoder.End();

            graphicsDevice.SubmitCommands(commandEncoder);

            graphicsDevice.DisposeWhenIdle(commandEncoder);

            foreach (var materialPass in materialPasses)
            {
                AddDisposable(materialPass);

                foreach (var meshPart in materialPass.MeshParts)
                {
                    AddDisposable(meshPart.Material);
                    meshPart.Material.SetMeshConstants(_meshConstantsBuffer.Buffer);

                    AddDisposable(meshPart.DepthMaterial);
                    meshPart.DepthMaterial.SetMeshConstants(_meshConstantsBuffer.Buffer);
                }
            }
            MaterialPasses = materialPasses;
        }

        internal void BuildRenderList(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
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
                parentBone,
                meshWorldMatrix,
                castsShadow,
                owner);
        }

        internal void BuildRenderListWithWorldMatrix(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
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

            foreach (var materialPass in MaterialPasses)
            {
                foreach (var meshPart in materialPass.MeshParts)
                {
                    var blendEnabled = meshPart.Material.PipelineState.BlendState.AttachmentStates[0].BlendEnabled;

                    // Depth pass

                    // TODO: With more work, we could draw shadows for translucent and alpha-tested materials.
                    if (!blendEnabled && castsShadow)
                    {
                        meshPart.DepthMaterial.SetSkinningBuffer(modelInstance.SkinningBuffer);

                        renderList.Shadow.RenderItems.Add(new RenderItem(
                           meshPart.DepthMaterial,
                           _vertexBuffer,
                           materialPass.TexCoordVertexBuffer,
                           CullFlags.None,
                           meshBoundingBox,
                           world,
                           meshPart.StartIndex,
                           meshPart.IndexCount,
                           _indexBuffer));
                    }

                    // Standard pass

                    meshPart.Material.SetSkinningBuffer(modelInstance.SkinningBuffer);

                    var renderQueue = blendEnabled
                        ? renderList.Transparent
                        : renderList.Opaque;

                    renderQueue.RenderItems.Add(new RenderItem(
                        meshPart.Material,
                        _vertexBuffer,
                        materialPass.TexCoordVertexBuffer,
                        CullFlags.None,
                        meshBoundingBox,
                        world,
                        meshPart.StartIndex,
                        meshPart.IndexCount,
                        _indexBuffer,
                        owner?.Color));
                }
            }
        }
    }
}

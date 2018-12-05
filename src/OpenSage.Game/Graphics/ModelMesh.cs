using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
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

        private readonly Effect _effect;

        public string Name { get; }

        public ModelBone ParentBone { get; }
        public uint NumBones { get; }

        public BoundingBox BoundingBox { get; }

        public ModelMeshMaterialPass[] MaterialPasses { get; }

        public bool Skinned { get; }

        public bool Hidden { get; }
        public bool CameraOriented { get; }

        public bool HasHouseColor { get; }
        public Vector3 HouseColor { get; }

        internal ModelMesh(
            GraphicsDevice graphicsDevice,
            string name,
            ReadOnlySpan<byte> vertexData,
            ushort[] indices,
            Effect effect,
            ModelMeshMaterialPass[] materialPasses,
            bool isSkinned,
            ModelBone parentBone,
            uint numBones,
            BoundingBox boundingBox,
            bool hidden,
            bool cameraOriented,
            Vector3? houseColor)
        {
            Name = name;

            ParentBone = parentBone;
            NumBones = numBones;

            BoundingBox = boundingBox;

            Skinned = isSkinned;

            if (HasHouseColor = houseColor.HasValue)
            {
                HouseColor = houseColor.Value;
            }

            Hidden = hidden;
            CameraOriented = cameraOriented;

            _effect = effect;

            _vertexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(vertexData, BufferUsage.VertexBuffer));

            _indexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                indices,
                BufferUsage.IndexBuffer));

            var commandEncoder = graphicsDevice.ResourceFactory.CreateCommandList();

            commandEncoder.Begin();

            _meshConstantsBuffer = AddDisposable(new ConstantBuffer<MeshMaterial.MeshConstants>(graphicsDevice));
            _meshConstantsBuffer.Value.SkinningEnabled = isSkinned;
            _meshConstantsBuffer.Value.NumBones = numBones;
            _meshConstantsBuffer.Value.HasHouseColor = houseColor.HasValue;
            if (houseColor.HasValue)
            {
                _meshConstantsBuffer.Value.HouseColor = houseColor.Value;
            }
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
            in Matrix4x4 modelTransform,
            bool castsShadow)
        {
            var meshWorldMatrix = Skinned
                ? modelTransform
                : modelInstance.AbsoluteBoneTransforms[ParentBone.Index];

            BuildRenderListWithWorldMatrix(
                renderList,
                camera,
                modelInstance,
                meshWorldMatrix,
                castsShadow);
        }

        internal void BuildRenderListWithWorldMatrix(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            in Matrix4x4 meshWorldMatrix,
            bool castsShadow)
        {
            if (Hidden)
            {
                return;
            }

            if (!modelInstance.BoneVisibilities[ParentBone.Index])
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

                        renderList.Shadow.AddRenderItemDrawIndexed(
                           meshPart.DepthMaterial,
                           _vertexBuffer,
                           materialPass.TexCoordVertexBuffer,
                           CullFlags.None,
                           meshBoundingBox,
                           world,
                           meshPart.StartIndex,
                           meshPart.IndexCount,
                           _indexBuffer);
                    }

                    // Standard pass

                    meshPart.Material.SetSkinningBuffer(modelInstance.SkinningBuffer);

                    var renderQueue = blendEnabled
                        ? renderList.Transparent
                        : renderList.Opaque;

                    renderQueue.AddRenderItemDrawIndexed(
                        meshPart.Material,
                        _vertexBuffer,
                        materialPass.TexCoordVertexBuffer,
                        CullFlags.None,
                        meshBoundingBox,
                        world,
                        meshPart.StartIndex,
                        meshPart.IndexCount,
                        _indexBuffer);
                }
            }
        }
    }
}

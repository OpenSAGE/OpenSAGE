using System.Numerics;
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
        internal const int MaxBones = 100;

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

        public ModelMesh(
            GraphicsDevice graphicsDevice,
            string name,
            MeshVertex.Basic[] vertices,
            ushort[] indices,
            Effect effect,
            ModelMeshMaterialPass[] materialPasses,
            bool isSkinned,
            ModelBone parentBone,
            uint numBones,
            BoundingBox boundingBox,
            bool hidden,
            bool cameraOriented)
        {
            Name = name;

            ParentBone = parentBone;
            NumBones = numBones;

            BoundingBox = boundingBox;

            Skinned = isSkinned;

            Hidden = hidden;
            CameraOriented = cameraOriented;

            _effect = effect;

            _vertexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                vertices,
                BufferUsage.VertexBuffer));

            _indexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                indices,
                BufferUsage.IndexBuffer));

            var commandEncoder = graphicsDevice.ResourceFactory.CreateCommandList();

            commandEncoder.Begin();

            _meshConstantsBuffer = AddDisposable(new ConstantBuffer<MeshMaterial.MeshConstants>(graphicsDevice));
            _meshConstantsBuffer.Value.SkinningEnabled = isSkinned;
            _meshConstantsBuffer.Value.NumBones = numBones;
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
                }
            }
            MaterialPasses = materialPasses;
        }

        internal void BuildRenderList(RenderList renderList, MeshComponent mesh)
        {
            if (Hidden)
            {
                return;
            }

            Matrix4x4 world;
            if (CameraOriented)
            {
                var localToWorldMatrix = mesh.Transform.LocalToWorldMatrix;

                var viewInverse = Matrix4x4Utility.Invert(mesh.Game.Scene.Camera.View);
                var cameraPosition = viewInverse.Translation;

                var toCamera = Vector3.Normalize(Vector3.TransformNormal(
                    cameraPosition - mesh.Transform.WorldPosition,
                    mesh.Transform.WorldToLocalMatrix));

                toCamera.Z = 0;

                var cameraOrientedRotation = Matrix4x4.CreateFromQuaternion(QuaternionUtility.CreateRotation(Vector3.UnitX, toCamera));

                world = cameraOrientedRotation * localToWorldMatrix;
            }
            else
            {
                world = mesh.Transform.LocalToWorldMatrix;
            }

            ModelComponent modelComponent = null;
            if (Skinned)
            {
                modelComponent = mesh.Entity.GetComponent<ModelComponent>();
            }

            foreach (var materialPass in MaterialPasses)
            {
                foreach (var meshPart in materialPass.MeshParts)
                {
                    meshPart.Material.SetSkinningBuffer(modelComponent?.SkinningBuffer);

                    var renderQueue = meshPart.Material.PipelineState.BlendState.AttachmentStates[0].BlendEnabled
                        ? renderList.Transparent
                        : renderList.Opaque;

                    renderQueue.AddRenderItemDrawIndexed(
                        meshPart.Material,
                        _vertexBuffer,
                        materialPass.TexCoordVertexBuffer,
                        CullFlags.None,
                        mesh,
                        world,
                        meshPart.StartIndex,
                        meshPart.IndexCount,
                        _indexBuffer);
                }
            }
        }
    }
}

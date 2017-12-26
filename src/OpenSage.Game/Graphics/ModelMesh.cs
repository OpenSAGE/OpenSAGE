using System.Numerics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

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
    public sealed class ModelMesh : GraphicsObject
    {
        internal const int MaxBones = 100;

        private readonly Buffer<MeshVertex.Basic> _vertexBuffer;
        private readonly Buffer<ushort> _indexBuffer;

        private readonly ConstantBuffer<MeshMaterial.MeshConstants> _meshConstantsBuffer;

        private readonly Buffer<Matrix4x3> _skinningBuffer;
        private readonly Matrix4x3[] _skinningBones;

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

            _vertexBuffer = AddDisposable(Buffer<MeshVertex.Basic>.CreateStatic(
                graphicsDevice,
                vertices,
                BufferBindFlags.VertexBuffer));

            _indexBuffer = AddDisposable(Buffer<ushort>.CreateStatic(
                graphicsDevice,
                indices,
                BufferBindFlags.IndexBuffer));

            _meshConstantsBuffer = AddDisposable(new ConstantBuffer<MeshMaterial.MeshConstants>(graphicsDevice));
            _meshConstantsBuffer.Value.SkinningEnabled = isSkinned;
            _meshConstantsBuffer.Value.NumBones = numBones;

            if (isSkinned)
            {
                _skinningBuffer = AddDisposable(Buffer<Matrix4x3>.CreateDynamicArray(
                    graphicsDevice,
                    (int) numBones,
                    BufferBindFlags.ShaderResource));

                _skinningBones = new Matrix4x3[numBones];
            }

            foreach (var materialPass in materialPasses)
            {
                AddDisposable(materialPass);

                foreach (var meshPart in materialPass.MeshParts)
                {
                    meshPart.Material.SetSkinningBuffer(_skinningBuffer);
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

            if (Skinned)
            {
                var bones = mesh.Entity.GetComponent<ModelComponent>().Bones;

                for (var i = 0; i < NumBones; i++)
                {
                    // Bone matrix should be relative to root bone transform.
                    var rootBoneMatrix = bones[0].LocalToWorldMatrix;
                    var boneMatrix = bones[i].LocalToWorldMatrix;

                    var boneMatrixRelativeToRoot = boneMatrix * Matrix4x4Utility.Invert(rootBoneMatrix);

                    boneMatrixRelativeToRoot.ToMatrix4x3(out _skinningBones[i]);
                }

                _skinningBuffer.SetData(_skinningBones);
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

            _meshConstantsBuffer.Value.World = world;
            _meshConstantsBuffer.Update();

            foreach (var materialPass in MaterialPasses)
            {
                foreach (var meshPart in materialPass.MeshParts)
                {
                    var renderQueue = meshPart.Material.PipelineState.BlendState.Enabled
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

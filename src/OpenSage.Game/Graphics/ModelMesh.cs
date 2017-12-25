using System.Collections.Generic;
using System.Linq;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using System.Numerics;
using OpenSage.Graphics.Cameras;

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
            }
            MaterialPasses = materialPasses;
        }

        internal void BuildRenderList(RenderList renderList, MeshComponent mesh)
        {
            if (Hidden)
            {
                return;
            }

            var uniquePipelineStates = MaterialPasses
                .SelectMany(x => x.MeshParts.Select(y => y.PipelineStateHandle))
                .Distinct()
                .ToList();

            foreach (var pipelineStateHandle in uniquePipelineStates)
            {
                var filteredMaterialPasses = MaterialPasses
                    .Where(x => x.MeshParts.Any(y => y.PipelineStateHandle == pipelineStateHandle))
                    .ToList();

                if (filteredMaterialPasses.Count > 0)
                {
                    renderList.AddRenderItem(new RenderItem(
                        mesh,
                        filteredMaterialPasses[0].MeshParts[0].Material, // TODO
                        pipelineStateHandle,
                        (commandEncoder, e, h, c) =>
                        {
                            Draw(
                                commandEncoder,
                                _effect,
                                h,
                                filteredMaterialPasses,
                                mesh,
                                c);
                        }));
                }
            }
        }

        private void Draw(
            CommandEncoder commandEncoder, 
            Effect effect,
            EffectPipelineStateHandle pipelineStateHandle,
            IEnumerable<ModelMeshMaterialPass> materialPasses,
            MeshComponent renderable,
            CameraComponent camera)
        {
            if (Skinned)
            {
                var bones = renderable.Entity.GetComponent<ModelComponent>().Bones;

                for (var i = 0; i < NumBones; i++)
                {
                    // Bone matrix should be relative to root bone transform.
                    var rootBoneMatrix = bones[0].LocalToWorldMatrix;
                    var boneMatrix = bones[i].LocalToWorldMatrix;

                    var boneMatrixRelativeToRoot = boneMatrix * Matrix4x4Utility.Invert(rootBoneMatrix);

                    boneMatrixRelativeToRoot.ToMatrix4x3(out _skinningBones[i]);
                }

                _skinningBuffer.SetData(_skinningBones);

                effect.SetValue("SkinningBuffer", _skinningBuffer);
            }

            Matrix4x4 world;
            if (CameraOriented)
            {
                var localToWorldMatrix = renderable.Transform.LocalToWorldMatrix;

                var viewInverse = Matrix4x4Utility.Invert(camera.View);
                var cameraPosition = viewInverse.Translation;

                var toCamera = Vector3.Normalize(Vector3.TransformNormal(
                    cameraPosition - renderable.Transform.WorldPosition,
                    renderable.Transform.WorldToLocalMatrix));

                toCamera.Z = 0;

                var cameraOrientedRotation = Matrix4x4.CreateFromQuaternion(QuaternionUtility.CreateRotation(Vector3.UnitX, toCamera));

                world = cameraOrientedRotation * localToWorldMatrix;
            }
            else
            {
                world = renderable.Transform.LocalToWorldMatrix;
            }

            _meshConstantsBuffer.Value.World = world;
            _meshConstantsBuffer.Update();

            effect.SetValue("MeshConstants", _meshConstantsBuffer.Buffer);

            commandEncoder.SetVertexBuffer(0, _vertexBuffer);

            foreach (var materialPass in materialPasses)
            {
                commandEncoder.SetVertexBuffer(1, materialPass.TexCoordVertexBuffer);

                foreach (var meshPart in materialPass.MeshParts)
                {
                    if (meshPart.PipelineStateHandle != pipelineStateHandle)
                    {
                        continue;
                    }

                    meshPart.Material.Apply();

                    effect.Apply(commandEncoder);

                    commandEncoder.DrawIndexed(
                        PrimitiveType.TriangleList,
                        meshPart.IndexCount,
                        _indexBuffer,
                        meshPart.StartIndex);
                }
            }
        }
    }
}

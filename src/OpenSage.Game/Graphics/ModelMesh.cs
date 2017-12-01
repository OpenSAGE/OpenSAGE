using System.Collections.Generic;
using System.Linq;
using LLGfx;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
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

        private readonly Buffer<MeshMaterial.SkinningConstants> _skinningConstantsBuffer;

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

            _skinningConstantsBuffer = AddDisposable(Buffer<MeshMaterial.SkinningConstants>.CreateStatic(
                graphicsDevice,
                new MeshMaterial.SkinningConstants
                {
                    SkinningEnabled = isSkinned,
                    NumBones = numBones
                },
                BufferBindFlags.ConstantBuffer));

            foreach (var materialPass in materialPasses)
            {
                AddDisposable(materialPass);
            }
            MaterialPasses = materialPasses;
        }

        internal void BuildRenderList(RenderList renderList, RenderInstanceData instanceData)
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
                    renderList.AddRenderItem(new InstancedRenderItem(
                        instanceData,
                        _effect,
                        pipelineStateHandle,
                        (commandEncoder, e, h, _) =>
                        {
                            Draw(
                                commandEncoder,
                                _effect,
                                h,
                                filteredMaterialPasses,
                                instanceData);
                        }));
                }
            }
        }

        private void Draw(
            CommandEncoder commandEncoder, 
            Effect effect,
            EffectPipelineStateHandle pipelineStateHandle,
            IEnumerable<ModelMeshMaterialPass> materialPasses,
            RenderInstanceData instanceData)
        {
            commandEncoder.SetVertexBuffer(2, instanceData.WorldBuffer);

            if (Skinned)
            {
                effect.SetValue("SkinningBuffer", instanceData.SkinningBuffer);
            }

            effect.SetValue("SkinningConstants", _skinningConstantsBuffer);

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

                    commandEncoder.DrawIndexedInstanced(
                        PrimitiveType.TriangleList,
                        meshPart.IndexCount,
                        instanceData.NumInstances,
                        _indexBuffer,
                        meshPart.StartIndex);
                }
            }
        }
    }
}

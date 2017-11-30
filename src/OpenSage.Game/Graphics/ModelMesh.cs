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

        private readonly Buffer<VertexMaterial> _materialsBuffer;

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
            VertexMaterial[] vertexMaterials,
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

            _vertexBuffer = AddDisposable(Buffer<MeshVertex.Basic>.CreateStatic(
                graphicsDevice,
                vertices,
                BufferBindFlags.VertexBuffer));

            _indexBuffer = AddDisposable(Buffer<ushort>.CreateStatic(
                graphicsDevice,
                indices,
                BufferBindFlags.IndexBuffer));

            _materialsBuffer = AddDisposable(Buffer<VertexMaterial>.CreateStatic(
                graphicsDevice,
                vertexMaterials,
                BufferBindFlags.ShaderResource));

            foreach (var materialPass in materialPasses)
            {
                AddDisposable(materialPass);
            }
            MaterialPasses = materialPasses;
        }

        internal void BuildRenderList(RenderList renderList, RenderInstanceData instanceData, MeshMaterial material)
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
                        material,
                        pipelineStateHandle,
                        (commandEncoder, e, h, _) =>
                        {
                            Draw(
                                commandEncoder,
                                material,
                                h,
                                filteredMaterialPasses,
                                instanceData);
                        }));
                }
            }
        }

        private void Draw(
            CommandEncoder commandEncoder, 
            MeshMaterial meshMaterial,
            EffectPipelineStateHandle pipelineStateHandle,
            IEnumerable<ModelMeshMaterialPass> materialPasses,
            RenderInstanceData instanceData)
        {
            // TODO
            var meshEffect = (MeshEffect) meshMaterial.Effect;

            commandEncoder.SetVertexBuffer(2, instanceData.WorldBuffer);

            if (Skinned)
            {
                meshEffect.SetSkinningBuffer(instanceData.SkinningBuffer);
                meshEffect.SetNumBones(NumBones);
            }

            meshEffect.SetSkinningEnabled(Skinned);

            meshEffect.SetMaterials(_materialsBuffer);

            commandEncoder.SetVertexBuffer(0, _vertexBuffer);

            foreach (var materialPass in materialPasses)
            {
                meshEffect.SetMaterialIndices(materialPass.MaterialIndicesBuffer);
                meshEffect.SetNumTextureStages(materialPass.NumTextureStages);

                commandEncoder.SetVertexBuffer(1, materialPass.TexCoordVertexBuffer);

                foreach (var meshPart in materialPass.MeshParts)
                {
                    if (meshPart.PipelineStateHandle != pipelineStateHandle)
                    {
                        continue;
                    }

                    meshEffect.SetShadingConfiguration(meshPart.ShadingConfiguration);

                    meshEffect.SetTexture0(meshPart.Texture0);
                    meshEffect.SetTexture1(meshPart.Texture1);

                    meshEffect.Apply(commandEncoder);

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

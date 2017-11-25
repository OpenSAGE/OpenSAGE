using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using LLGfx.Effects;
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

        private readonly StaticBuffer<MeshVertex> _vertexBuffer;
        private readonly StaticBuffer<ushort> _indexBuffer;

        private readonly StaticBuffer<VertexMaterial> _materialsBuffer;
        private readonly StaticBuffer<ShadingConfiguration> _shadingConfigurationsBuffer;

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
            ResourceUploadBatch uploadBatch,
            string name,
            MeshVertex[] vertices,
            ushort[] indices,
            VertexMaterial[] vertexMaterials,
            ModelMeshMaterialPass[] materialPasses,
            ShadingConfiguration[] shadingConfigurations,
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

            _vertexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertices));

            _indexBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                indices));

            _materialsBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertexMaterials));

            foreach (var materialPass in materialPasses)
            {
                AddDisposable(materialPass);
            }
            MaterialPasses = materialPasses;

            _shadingConfigurationsBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                shadingConfigurations));
        }

        internal void BuildRenderList(RenderList renderList, RenderInstanceData instanceData, MeshEffect effect)
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
                        effect,
                        pipelineStateHandle,
                        (commandEncoder, e, h, _) =>
                        {
                            Draw(
                                commandEncoder,
                                effect,
                                h,
                                filteredMaterialPasses,
                                instanceData);
                        }));
                }
            }
        }

        private void Draw(
            CommandEncoder commandEncoder, 
            MeshEffect meshEffect,
            EffectPipelineStateHandle pipelineStateHandle,
            IEnumerable<ModelMeshMaterialPass> materialPasses,
            RenderInstanceData instanceData)
        {
            commandEncoder.SetVertexBuffer(2, instanceData.WorldBuffer);

            if (Skinned)
            {
                meshEffect.SetSkinningBuffer(instanceData.SkinningBuffer);
                meshEffect.SetNumBones(NumBones);
            }

            meshEffect.SetSkinningEnabled(Skinned);

            meshEffect.SetMaterials(_materialsBuffer);
            meshEffect.SetShadingConfigurations(_shadingConfigurationsBuffer);

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

                    meshEffect.SetShadingConfigurationID(meshPart.ShadingConfigurationID);

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

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint BoneIndex;
    }
}

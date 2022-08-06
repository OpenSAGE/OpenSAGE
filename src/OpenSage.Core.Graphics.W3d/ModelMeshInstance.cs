using System.Collections.Generic;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshInstance : DisposableBase
    {
        private readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsVS> _renderItemConstantsBufferVS;
        private readonly ResourceSet _renderItemConstantsResourceSet;

        public readonly List<ModelMeshPartInstance> MeshPartInstances = new();

        public ModelMeshInstance(
            ModelMesh modelMesh,
            ModelInstance modelInstance,
            GraphicsDevice graphicsDevice,
            MeshShaderResources meshShaderResources)
        {
            _renderItemConstantsBufferVS = AddDisposable(
                new ConstantBuffer<MeshShaderResources.RenderItemConstantsVS>(
                    graphicsDevice,
                    $"{modelMesh.SubObject.FullName}_RenderItemConstantsVS"));

            _renderItemConstantsResourceSet = AddDisposable(
                meshShaderResources.CreateRenderItemConstantsResourceSet(
                    modelMesh.MeshConstantsBuffer,
                    _renderItemConstantsBufferVS,
                    modelInstance.SkinningBuffer,
                    modelInstance.RenderItemConstantsBufferPS));

            foreach (var modelMeshPart in modelMesh.MeshParts)
            {
                MeshPartInstances.Add(
                    AddDisposable(
                        new ModelMeshPartInstance(modelMeshPart, this)));
            }
        }

        internal void OnBeforeRender(
            CommandList cl,
            in RenderItem renderItem)
        {
            if (_renderItemConstantsBufferVS.Value.World != renderItem.World)
            {
                _renderItemConstantsBufferVS.Value.World = renderItem.World;
                _renderItemConstantsBufferVS.Update(cl);
            }

            cl.SetGraphicsResourceSet(3, _renderItemConstantsResourceSet);
        }
    }
}

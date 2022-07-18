using System.Collections.Generic;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class ModelMeshInstance : DisposableBase
    {
        internal readonly ConstantBuffer<MeshShaderResources.RenderItemConstantsVS> RenderItemConstantsBufferVS;

        public readonly ModelInstance ModelInstance;

        public readonly List<ModelMeshPartInstance> MeshPartInstances = new();

        public ModelMeshInstance(
            ModelMesh modelMesh,
            ModelInstance modelInstance,
            GraphicsDevice graphicsDevice,
            MeshShaderResources meshShaderResources)
        {
            ModelInstance = modelInstance;

            RenderItemConstantsBufferVS = AddDisposable(
                new ConstantBuffer<MeshShaderResources.RenderItemConstantsVS>(
                    graphicsDevice,
                    $"{modelMesh.SubObject.FullName}_RenderItemConstantsVS"));

            foreach (var modelMeshPart in modelMesh.MeshParts)
            {
                MeshPartInstances.Add(
                    AddDisposable(
                        new ModelMeshPartInstance(
                            modelMeshPart,
                            this,
                            meshShaderResources)));
            }
        }

        internal void OnBeforeRender(
            CommandList cl,
            in RenderItem renderItem)
        {
            if (RenderItemConstantsBufferVS.Value.World != renderItem.World)
            {
                RenderItemConstantsBufferVS.Value.World = renderItem.World;
                RenderItemConstantsBufferVS.Update(cl);
            }
        }
    }
}

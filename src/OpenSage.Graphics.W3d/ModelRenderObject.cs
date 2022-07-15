using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public abstract class ModelRenderObject : BaseAsset
    {
        public abstract ref readonly AxisAlignedBoundingBox BoundingBox { get; }

        public abstract bool Hidden { get; }

        internal ModelSubObject SubObject;

        public abstract void BuildRenderList(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            ModelMeshInstance modelMeshInstance,
            ModelBone parentBone,
            in Matrix4x4 modelTransform,
            bool castsShadow,
            MeshShaderResources.RenderItemConstantsPS? renderItemConstantsPS);

        public abstract void BuildRenderListWithWorldMatrix(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            ModelMeshInstance modelMeshInstance,
            ModelBone parentBone,
            in Matrix4x4 meshWorldMatrix,
            bool castsShadow,
            MeshShaderResources.RenderItemConstantsPS? renderItemConstantsPS = null);
    }
}

using System.Diagnostics;
using OpenSage.Core.Graphics;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics
{
    [DebuggerDisplay("Model '{Name}'")]
    public sealed class Model : BaseAsset
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;

        public readonly ModelBoneHierarchy BoneHierarchy;
        public readonly ModelSubObject[] SubObjects;

        public readonly bool HasSkinnedMeshes;

        public Model(
            string name,
            ModelBoneHierarchy boneHierarchy,
            ModelSubObject[] subObjects,
            GraphicsDeviceManager graphicsDeviceManager)
        {
            SetNameAndInstanceId("W3DContainer", name);

            _graphicsDeviceManager = graphicsDeviceManager;

            BoneHierarchy = boneHierarchy;
            SubObjects = subObjects;

            foreach (var subObject in subObjects)
            {
                if (subObject.RenderObject is ModelMesh modelMesh && modelMesh.Skinned)
                {
                    HasSkinnedMeshes = true;
                    break;
                }
            }
        }

        public ModelInstance CreateInstance()
        {
            return new ModelInstance(this, _graphicsDeviceManager);
        }
    }
}

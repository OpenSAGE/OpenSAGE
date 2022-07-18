using System.Diagnostics;
using OpenSage.Graphics.Shaders;
using Veldrid;

namespace OpenSage.Graphics
{
    [DebuggerDisplay("Model '{Name}'")]
    public sealed class Model : BaseAsset
    {
        public readonly ModelBoneHierarchy BoneHierarchy;
        public readonly ModelSubObject[] SubObjects;

        public readonly bool HasSkinnedMeshes;

        public Model(
            string name,
            ModelBoneHierarchy boneHierarchy,
            ModelSubObject[] subObjects)
            : this(boneHierarchy, subObjects)
        {
            SetNameAndInstanceId("W3DContainer", name);
        }

        private Model(
            ModelBoneHierarchy boneHierarchy,
            ModelSubObject[] subObjects)
        {
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

        public ModelInstance CreateInstance(
            GraphicsDevice graphicsDevice,
            StandardGraphicsResources standardGraphicsResources,
            MeshShaderResources meshShaderResources)
        {
            return new ModelInstance(
                this,
                graphicsDevice,
                standardGraphicsResources,
                meshShaderResources);
        }
    }
}

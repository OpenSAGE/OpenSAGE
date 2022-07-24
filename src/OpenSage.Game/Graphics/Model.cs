using System.Diagnostics;
using OpenSage.Content.Loaders;

namespace OpenSage.Graphics
{
    [DebuggerDisplay("Model '{Name}'")]
    public sealed class Model : BaseAsset
    {
        public readonly ModelBoneHierarchy BoneHierarchy;
        public readonly ModelSubObject[] SubObjects;

        public readonly bool HasSkinnedMeshes;

        internal Model(
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

        internal ModelInstance CreateInstance(AssetLoadContext loadContext)
        {
            return new ModelInstance(this, loadContext);
        }
    }
}

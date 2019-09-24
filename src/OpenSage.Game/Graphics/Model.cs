using OpenSage.Content.Loaders;
using OpenSage.Data.StreamFS;

namespace OpenSage.Graphics
{
    public sealed class Model : BaseAsset
    {
        public readonly ModelBoneHierarchy BoneHierarchy;
        public readonly ModelSubObject[] SubObjects;

        internal Model(
            string name,
            ModelBoneHierarchy boneHierarchy,
            ModelSubObject[] subObjects)
            : this(boneHierarchy, subObjects)
        {
            SetNameAndInstanceId("W3DContainer", name);
        }

        internal Model(
            Asset asset,
            ModelBoneHierarchy boneHierarchy,
            ModelSubObject[] subObjects)
            : this(boneHierarchy, subObjects)
        {
            SetNameAndInstanceId(asset);
        }

        private Model(
            ModelBoneHierarchy boneHierarchy,
            ModelSubObject[] subObjects)
        {
            BoneHierarchy = boneHierarchy;
            SubObjects = subObjects;
        }

        internal ModelInstance CreateInstance(AssetLoadContext loadContext)
        {
            return new ModelInstance(this, loadContext);
        }
    }
}

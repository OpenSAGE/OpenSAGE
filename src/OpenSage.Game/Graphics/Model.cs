using OpenSage.Content.Loaders;

namespace OpenSage.Graphics
{
    public sealed class Model : DisposableBase
    {
        public string Name { get; }
        public readonly ModelBoneHierarchy BoneHierarchy;
        public readonly ModelSubObject[] SubObjects;

        internal Model(
            string name,
            ModelBoneHierarchy boneHierarchy,
            ModelSubObject[] subObjects)
        {
            Name = name;
            BoneHierarchy = boneHierarchy;

            foreach (var subObject in subObjects)
            {
                AddDisposable(subObject.RenderObject);
            }
            SubObjects = subObjects;
        }

        internal ModelInstance CreateInstance(AssetLoadContext loadContext)
        {
            return new ModelInstance(this, loadContext);
        }
    }

    public sealed class ModelSubObject
    {
        public readonly string Name;
        public readonly ModelBone Bone;
        public readonly ModelMesh RenderObject;

        internal ModelSubObject(string name, ModelBone bone, ModelMesh renderObject)
        {
            Name = name;
            Bone = bone;
            RenderObject = renderObject;
        }
    }
}

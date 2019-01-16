using OpenSage.Content;

namespace OpenSage.Graphics
{
    public sealed class Model : DisposableBase
    {
        // TODO: Remove this.
        public readonly string Name;
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

        public ModelInstance CreateInstance(ContentManager contentManager)
        {
            return new ModelInstance(this, contentManager);
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

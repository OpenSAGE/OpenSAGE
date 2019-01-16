using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class Model : DisposableBase
    {
        // TODO: Remove this.
        public string Name { get; }
        public ModelBoneHierarchy BoneHierarchy { get; }
        public ModelSubObject[] SubObjects { get; }

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
        public string Name { get; }
        public ModelBone Bone { get; }
        public ModelMesh RenderObject { get; }

        internal ModelSubObject(string name, ModelBone bone, ModelMesh renderObject)
        {
            Name = name;
            Bone = bone;
            RenderObject = renderObject;
        }
    }
}

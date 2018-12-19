using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class Model : DisposableBase
    {
        public ModelBoneHierarchy BoneHierarchy { get; }
        public ModelSubObject[] SubObjects { get; }

        internal Model(
            ModelBoneHierarchy boneHierarchy,
            ModelSubObject[] subObjects)
        {
            BoneHierarchy = boneHierarchy;

            foreach (var subObject in subObjects)
            {
                AddDisposable(subObject.RenderObject);
            }
            SubObjects = subObjects;
        }

        public ModelInstance CreateInstance(GraphicsDevice graphicsDevice)
        {
            return new ModelInstance(this, graphicsDevice);
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

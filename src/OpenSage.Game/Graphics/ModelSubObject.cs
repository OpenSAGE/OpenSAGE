namespace OpenSage.Graphics
{
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

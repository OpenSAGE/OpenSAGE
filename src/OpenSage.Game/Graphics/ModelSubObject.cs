using System.Diagnostics;

namespace OpenSage.Graphics
{
    [DebuggerDisplay("ModelSubOject '{Name}'")]
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

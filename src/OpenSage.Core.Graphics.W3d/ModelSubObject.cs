using System.Diagnostics;

namespace OpenSage.Graphics
{
    [DebuggerDisplay("ModelSubOject '{Name}'")]
    public sealed class ModelSubObject
    {
        public readonly string FullName;
        public readonly string Name;
        public readonly ModelBone Bone;
        public readonly ModelRenderObject RenderObject;

        public ModelSubObject(string fullName, string name, ModelBone bone, ModelRenderObject renderObject)
        {
            FullName = fullName;
            Name = name;
            Bone = bone;
            RenderObject = renderObject;

            RenderObject.SubObject = this;
        }

        public ModelSubObject(string fullName, ModelBone bone, ModelRenderObject renderObject)
            : this(fullName, fullName.Substring(fullName.IndexOf('.') + 1), bone, renderObject)
        {

        }
    }
}

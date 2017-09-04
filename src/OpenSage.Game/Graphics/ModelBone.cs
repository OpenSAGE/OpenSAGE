using System.Collections.Generic;
using System.Numerics;

namespace OpenSage.Graphics
{
    public sealed class ModelBone
    {
        public int Index { get; }

        public string Name { get; }

        public ModelBone Parent { get; }

        public Matrix4x4 Transform { get; set; }

        public ModelBoneCollection Children { get; internal set; }

        internal ModelBone(int index, string name, ModelBone parent, Matrix4x4 transform)
        {
            Index = index;
            Name = name;
            Parent = parent;
            Transform = transform;
        }
    }
}

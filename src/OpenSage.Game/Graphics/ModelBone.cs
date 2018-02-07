using System.Numerics;

namespace OpenSage.Graphics
{
    public sealed class ModelBone
    {
        public int Index { get; }

        public string Name { get; }

        public ModelBone Parent { get; }

        public Transform Transform { get; }

        internal ModelBone(int index, string name, ModelBone parent, in Vector3 translation, in Quaternion rotation)
        {
            Index = index;
            Name = name;
            Parent = parent;

            Transform = new Transform(translation, rotation);
        }
    }
}

using System.Numerics;
using OpenSage.FileFormats.W3d;

namespace OpenSage.Graphics
{
    public sealed class ModelBoneHierarchy
    {
        public static ModelBoneHierarchy CreateDefault()
        {
            var bones = new ModelBone[1];
            bones[0] = new ModelBone(0, null, null, Vector3.Zero, Quaternion.Identity);
            return new ModelBoneHierarchy("[Default]", bones);
        }

        public string Name { get; }
        public ModelBone[] Bones { get; }

        internal ModelBoneHierarchy(string name, ModelBone[] bones)
        {
            Name = name;
            Bones = bones;
        }

        internal ModelBoneHierarchy(W3dHierarchyDef w3dHierarchy)
        {
            Bones = new ModelBone[w3dHierarchy.Pivots.Items.Count];

            for (var i = 0; i < w3dHierarchy.Pivots.Items.Count; i++)
            {
                var pivot = w3dHierarchy.Pivots.Items[i];

                var parent = pivot.ParentIdx == -1
                    ? null
                    : Bones[pivot.ParentIdx];

                Bones[i] = new ModelBone(
                    i,
                    pivot.Name,
                    parent,
                    pivot.Translation,
                    pivot.Rotation);
            }
        }
    }
}

using System.IO;
using System.Numerics;
using OpenSage.Data.StreamFS;
using OpenSage.Data.W3x;
using OpenSage.FileFormats.W3d;

namespace OpenSage.Graphics
{
    public sealed class ModelBoneHierarchy : BaseAsset
    {
        internal static ModelBoneHierarchy ParseAsset(BinaryReader reader, Asset asset)
        {
            var w3xHierarchy = W3xHierarchy.Parse(reader, asset.Header);

            var bones = new ModelBone[w3xHierarchy.Pivots.Length];

            for (var i = 0; i < w3xHierarchy.Pivots.Length; i++)
            {
                var w3xPivot = w3xHierarchy.Pivots[i];

                bones[i] = new ModelBone(
                    i,
                    w3xPivot.Name,
                    w3xPivot.ParentIdx != -1
                        ? bones[w3xPivot.ParentIdx]
                        : null,
                    w3xPivot.Translation,
                    w3xPivot.Rotation);

            }

            return new ModelBoneHierarchy(asset, bones);
        }

        public static ModelBoneHierarchy CreateDefault()
        {
            var bones = new ModelBone[1];
            bones[0] = new ModelBone(0, null, null, Vector3.Zero, Quaternion.Identity);
            return new ModelBoneHierarchy("[Default]", bones);
        }

        public ModelBone[] Bones { get; }

        private ModelBoneHierarchy(string name, ModelBone[] bones)
        {
            SetNameAndInstanceId("W3DHierarchy", name);
            Bones = bones;
        }

        internal ModelBoneHierarchy(Asset asset, ModelBone[] bones)
        {
            SetNameAndInstanceId(asset);
            Bones = bones;
        }

        internal ModelBoneHierarchy(W3dHierarchyDef w3dHierarchy)
        {
            SetNameAndInstanceId("W3DHierarchy", w3dHierarchy.Header.Name);

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

using System.IO;
using OpenSage.Data.W3x;
using OpenSage.Graphics;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class W3dHierarchyReader : AssetReader
    {
        public override AssetType AssetType => AssetType.W3dHierarchy;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
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
    }
}

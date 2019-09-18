using System.IO;
using OpenSage.Data.W3x;
using OpenSage.FileFormats;
using OpenSage.Graphics;

namespace OpenSage.Data.StreamFS.AssetReaders
{
    public sealed class W3dContainerReader : AssetReader
    {
        public override AssetType AssetType => AssetType.W3dContainer;

        public override object Parse(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context)
        {
            var hierarchy = imports.GetImportedData<ModelBoneHierarchy>(reader);
            var subObjects = reader.ReadArrayAtOffset(() => W3xSubObject.Parse(reader, imports));

            var modelSubObjects = new ModelSubObject[subObjects.Length];
            for (var i = 0; i < subObjects.Length; i++)
            {
                var subObject = subObjects[i];
                modelSubObjects[i] = new ModelSubObject(
                    subObject.Name,
                    hierarchy.Bones[subObject.BoneIndex],
                    subObject.RenderObject);
            }

            var model = new Model(asset.Name, hierarchy, modelSubObjects);

            context.AssetStore.Models.Add(model);

            return model;
        }
    }
}

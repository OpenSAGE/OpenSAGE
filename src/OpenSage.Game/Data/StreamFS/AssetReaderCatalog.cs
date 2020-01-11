using System.Collections.Generic;
using System.IO;
using OpenSage.Audio;
using OpenSage.Data.W3x;
using OpenSage.Graphics;
using OpenSage.Gui;

namespace OpenSage.Data.StreamFS
{
    public static class AssetReaderCatalog
    {
        private static readonly Dictionary<AssetType, ParseAssetDelegate> AssetReaders = new Dictionary<AssetType, ParseAssetDelegate>
        {
            { AssetType.AudioEvent, (asset, reader, imports, context) => AudioEvent.ParseAsset(reader, asset, imports) },
            { AssetType.AudioFile, (asset, reader, imports, context) => AudioFile.ParseAsset(reader, asset) },
            { AssetType.AudioSettings, (asset, reader, imports, context) => AudioSettings.ParseAsset(reader, asset) },
            { AssetType.CrowdResponse, (asset, reader, imports, context) => CrowdResponse.ParseAsset(reader, asset, imports) },
            { AssetType.DialogEvent, (asset, reader, imports, context) => DialogEvent.ParseAsset(reader, asset, imports) },
            { AssetType.MusicTrack, (asset, reader, imports, context) => MusicTrack.ParseAsset(reader, asset, imports) },
            { AssetType.PackedTextureImage, (asset, reader, imports, context) => MappedImage.ParseAsset(reader, asset, imports) },
            { AssetType.Texture, (asset, reader, imports, context) => TextureAsset.ParseAsset(reader, asset, context) },
            { AssetType.W3dCollisionBox, (asset, reader, imports, context) => W3xBox.Parse(reader) }, // TODO
            { AssetType.W3dContainer, (asset, reader, imports, context) => Model.ParseAsset(reader, asset, imports) },
            { AssetType.W3dHierarchy, (asset, reader, imports, context) => ModelBoneHierarchy.ParseAsset(reader, asset) },
            { AssetType.W3dMesh, (asset, reader, imports, context) => ModelMesh.ParseAsset(reader, asset, imports, context) },
        };

        public static bool TryGetAssetReader(uint typeId, out ParseAssetDelegate assetReader)
        {
            return AssetReaders.TryGetValue((AssetType) typeId, out assetReader);
        }
    }

    public delegate object ParseAssetDelegate(Asset asset, BinaryReader reader, AssetImportCollection imports, AssetParseContext context);
}

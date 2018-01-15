using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSage.Data.StreamFS
{
    public sealed class GameStream
    {
        private readonly Dictionary<AssetReference, Asset> _assetReferenceToAssetLookup;

        public FileSystemEntry ManifestFileEntry { get; }
        public ManifestFile ManifestFile { get; }

        public GameStream(FileSystemEntry manifestFileEntry)
        {
            ManifestFileEntry = manifestFileEntry;
            ManifestFile = ManifestFile.FromFileSystemEntry(manifestFileEntry);

            _assetReferenceToAssetLookup = new Dictionary<AssetReference, Asset>();
            foreach (var asset in ManifestFile.Assets)
            {
                var assetReference = new AssetReference(
                    asset.Header.TypeId,
                    asset.Header.InstanceId);
                _assetReferenceToAssetLookup[assetReference] = asset;
            }

            ParseStreamFile(".bin", reader =>
            {
                foreach (var asset in ManifestFile.Assets)
                {
                    if (AssetTypeCatalog.TryGetAssetType(asset.Header.TypeId, out var assetType))
                    {
                        asset.InstanceData = assetType.Parse(asset, reader);
                    }
                    else
                    {
                        asset.InstanceData = reader.ReadBytes((int) asset.Header.InstanceDataSize);
                    }
                }
            });

            // Relocations
            ParseStreamFile(".relo", reader =>
            {
                foreach (var asset in ManifestFile.Assets)
                {
                    if (asset.Header.RelocationDataSize != 0)
                    {
                        var numRelocations = asset.Header.RelocationDataSize / sizeof(uint) - 1;
                        asset.Relocations = new uint[numRelocations];
                        for (var i = 0; i < numRelocations; i++)
                        {
                            asset.Relocations[i] = reader.ReadUInt32();
                        }
                        var lastValue = reader.ReadUInt32();
                        if (lastValue != 0xFFFFFFFF)
                        {
                            throw new InvalidDataException();
                        }
                    }
                }
            });

            // Imports
            ParseStreamFile(".imp", reader =>
            {
                foreach (var asset in ManifestFile.Assets)
                {
                    if (asset.Header.ImportsDataSize > 0)
                    {
                        var numImports = asset.Header.ImportsDataSize / sizeof(uint) - 1;
                        asset.Imports = new AssetImport[numImports];
                        for (var i = 0; i < numImports; i++)
                        {
                            var importData = reader.ReadUInt32();

                            ref var assetReference = ref asset.AssetReferences[i];
                            var referencedAsset = FindAsset(assetReference);

                            asset.Imports[i] = new AssetImport(importData, referencedAsset);
                        }
                        var lastValue = reader.ReadUInt32();
                        if (lastValue != 0xFFFFFFFF)
                        {
                            throw new InvalidDataException();
                        }
                    }
                }
            });
        }

        private void ParseStreamFile(string extension, Action<BinaryReader> callback)
        {
            var streamFilePath = Path.ChangeExtension(ManifestFileEntry.FilePath, extension);
            var streamFileEntry = ManifestFileEntry.FileSystem.GetFile(streamFilePath);
            if (streamFileEntry == null)
            {
                return;
            }

            using (var binaryStream = streamFileEntry.Open())
            using (var reader = new BinaryReader(binaryStream, Encoding.ASCII, true))
            {
                var checksum = reader.ReadUInt32();
                if (checksum != ManifestFile.Header.StreamChecksum)
                {
                    throw new InvalidDataException();
                }

                callback(reader);
            }
        }

        public Asset FindAsset(in AssetReference reference)
        {
            _assetReferenceToAssetLookup.TryGetValue(reference, out var result);
            return result;
        }
    }
}

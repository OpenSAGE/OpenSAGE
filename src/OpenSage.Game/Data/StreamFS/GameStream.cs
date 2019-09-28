﻿using System;
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

        public GameStream(FileSystemEntry manifestFileEntry, Game game)
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

            var assetParseContext = new AssetParseContext(game);

            // Parse .bin, .relo, and .imp files simultaneously.
            ParseStreamFile(".bin", 3132817408u, binReader =>
            {
                ParseStreamFile(".relo", 3133014016u, reloReader =>
                {
                    ParseStreamFile(".imp", 3132162048u, impReader =>
                    {
                        foreach (var asset in ManifestFile.Assets)
                        {
                            ReadBinReloImpData(
                                binReader,
                                reloReader,
                                impReader,
                                asset,
                                out var instanceData,
                                out var relocationData,
                                out var imports);

                            asset.AssetImports = imports;

                            if (instanceData.Length == 0)
                            {
                                continue;
                            }

                            if (Enum.IsDefined(typeof(AssetType), asset.Header.TypeId)) // TODO: Remove this.
                            {
                                if (AssetReaderCatalog.TryGetAssetReader(asset.Header.TypeId, out var assetReader))
                                {
                                    using (var instanceDataStream = new MemoryStream(instanceData, false))
                                    using (var instanceDataReader = new BinaryReader(instanceDataStream, Encoding.ASCII, true))
                                    {
                                        var zero = instanceDataReader.ReadUInt32();
                                        if (zero != 0)
                                        {
                                            throw new InvalidDataException();
                                        }

                                        asset.InstanceData = assetReader(asset, instanceDataReader, imports, assetParseContext);

                                        var assetCollection = assetParseContext.AssetStore.GetAssetCollection(asset.Header.TypeId);
                                        if (assetCollection != null) // TODO: Eventually this shouldn't be null.
                                        {
                                            assetCollection.Add(asset.InstanceData);
                                        }
                                        else
                                        {
                                            var singleAssetStorage = assetParseContext.AssetStore.GetSingleAsset(asset.Header.TypeId);
                                            if (singleAssetStorage != null) // TODO: Eventually this shouldn't be null.
                                            {
                                                singleAssetStorage.Current = (BaseAsset) asset.InstanceData;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // TODO
                                }
                            }
                            else
                            {
                                // TODO
                                Logger.Info($"Missing AssetType: {asset.Name.Split(':')[0]} = 0x{asset.Header.TypeId.ToString("X")},");
                            }
                        }
                    });
                });
            });
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private void ReadBinReloImpData(
            BinaryReader binReader,
            BinaryReader reloReader,
            BinaryReader impReader,
            Asset asset,
            out byte[] instanceData,
            out uint[] relocationData,
            out AssetImportCollection imports)
        {
            if (asset.Header.InstanceDataSize > 0)
            {
                instanceData = binReader.ReadBytes((int) asset.Header.InstanceDataSize);
            }
            else
            {
                instanceData = new byte[0];
            }

            uint[] readRelocationOrImportData(BinaryReader reader, uint dataSize)
            {
                uint[] result;
                if (dataSize > 0)
                {
                    var numValues = dataSize / sizeof(uint) - 1;
                    result = new uint[numValues];
                    for (var i = 0; i < numValues; i++)
                    {
                        result[i] = reader.ReadUInt32();
                    }
                    var lastValue = reader.ReadUInt32();
                    if (lastValue != 0xFFFFFFFF)
                    {
                        throw new InvalidDataException();
                    }
                }
                else
                {
                    result = new uint[0];
                }
                return result;
            }

            // These are indices into InstanceData, and should be used to update
            // the pointer at InstanceData[reloValue],
            // from being relative to the start of InstanceData,
            // to being an absolute pointer.
            // In C++ it would be:
            // asset.InstanceData[reloValue] += asset.InstanceData;
            relocationData = readRelocationOrImportData(reloReader, asset.Header.RelocationDataSize);

            // List of indices in InstanceData at which to set pointers to the corresponding
            // assets in AssetReferences.
            var importData = readRelocationOrImportData(impReader, asset.Header.ImportsDataSize);
            if (importData.Length > asset.AssetReferences.Count)
            {
                throw new InvalidDataException();
            }
            var tempImports = new AssetImport[importData.Length];
            for (var i = 0; i < importData.Length; i++)
            {
                var assetReference = asset.AssetReferences[i];
                var referencedAsset = FindAsset(assetReference);

                tempImports[i] = new AssetImport(importData[i], referencedAsset);
            }
            imports = new AssetImportCollection(tempImports);
        }

        private void ParseStreamFile(string extension, uint streamTypeChecksum, Action<BinaryReader> callback)
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
                if (ManifestFile.Header.Version == 7)
                {
                    var typeChecksum = reader.ReadUInt32();
                    if (typeChecksum != streamTypeChecksum)
                    {
                        throw new InvalidDataException();
                    }
                }

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

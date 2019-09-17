using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using OpenSage.FileFormats;

namespace OpenSage.Data.StreamFS
{
    public sealed class ManifestFile
    {
        public ManifestHeader Header { get; private set; }
        public Asset[] Assets { get; private set; }
        public AssetReference[] AssetReferences { get; private set; }
        public IReadOnlyList<ManifestReference> ManifestReferences { get; private set; }

        public static ManifestFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var result = new ManifestFile
                {
                    Header = ManifestHeader.Parse(reader)
                };

                switch (result.Header.Version)
                {
                    case 5:
                    case 6:
                    case 7:
                        break;

                    default:
                        throw new InvalidDataException();
                }

                result.Assets = new Asset[result.Header.AssetCount];
                for (var i = 0; i < result.Assets.Length; i++)
                {
                    result.Assets[i] = new Asset
                    {
                        Header = AssetEntry.Parse(reader, result.Header.Version)
                    };
                }

                result.AssetReferences = new AssetReference[result.Header.AssetReferenceBufferSize / AssetReference.SizeInBytes];
                for (var i = 0; i < result.AssetReferences.Length; i++)
                {
                    result.AssetReferences[i] = AssetReference.Parse(reader);
                }

                foreach (var asset in result.Assets)
                {
                    asset.AssetReferences = new ArraySegment<AssetReference>(
                        result.AssetReferences,
                        (int) asset.Header.AssetReferenceOffset / AssetReference.SizeInBytes,
                        (int) asset.Header.AssetReferenceCount);
                }

                {
                    var endPosition = stream.Position + result.Header.ReferencedManifestNameBufferSize;
                    var manifestReferences = new List<ManifestReference>();

                    while (stream.Position < endPosition)
                    {
                        manifestReferences.Add(ManifestReference.Parse(reader));
                    }
                    result.ManifestReferences = manifestReferences;
                }

                {
                    var endPosition = stream.Position + result.Header.AssetNameBufferSize;
                    var assetNames = ReadNameBuffer(reader, endPosition);

                    foreach (var asset in result.Assets)
                    {
                        asset.Name = assetNames[asset.Header.NameOffset];
                    }
                }

                {
                    var endPosition = stream.Position + result.Header.SourceFileNameBufferSize;
                    var sourceFileNames = ReadNameBuffer(reader, endPosition);

                    foreach (var asset in result.Assets)
                    {
                        asset.SourceFileName = sourceFileNames[asset.Header.SourceFileNameOffset];
                    }
                }

                return result;
            }
        }

        private static Dictionary<uint, string> ReadNameBuffer(BinaryReader reader, long endPosition)
        {
            var names = new Dictionary<uint, string>();
            var nameOffset = 0u;
            while (reader.BaseStream.Position < endPosition)
            {
                var streamPos = reader.BaseStream.Position;
                names.Add(nameOffset, reader.ReadNullTerminatedString());
                nameOffset += (uint) (reader.BaseStream.Position - streamPos);
            }

            return names;
        }
    }

    public sealed class Asset
    {
        public AssetEntry Header { get; internal set; }

        public AssetType AssetType => (AssetType) Header.TypeId;

        public string Name { get; internal set; }
        public string SourceFileName { get; internal set; }

        public object InstanceData { get; internal set; }

        public IList<AssetReference> AssetReferences { get; internal set; }
        public AssetImportCollection AssetImports { get; internal set; }
    }

    public sealed class AssetImport
    {
        public uint InstanceDataIndex { get; }
        public Asset ImportedAsset { get; }

        internal AssetImport(uint instanceDataIndex, Asset importedAsset)
        {
            InstanceDataIndex = instanceDataIndex;
            ImportedAsset = importedAsset;
        }
    }

    public sealed class AssetImportCollection : ReadOnlyCollection<AssetImport>
    {
        public AssetImportCollection(IList<AssetImport> list)
            : base(list)
        {
        }

        public T GetImportedData<T>(BinaryReader reader)
        {
            var position = reader.BaseStream.Position;

            // I thought this might be the import index, but doesn't seem so.
            var unknown = reader.ReadUInt32();
            
            var import = this.First(x => x.InstanceDataIndex == position);

            return (T) import.ImportedAsset?.InstanceData;
        }
    }

    public readonly struct AssetReference
    {
        internal const int SizeInBytes = sizeof(uint) * 2;

        public readonly uint TypeId;
        public readonly uint InstanceId;

        internal AssetReference(uint typeId, uint instanceId)
        {
            TypeId = typeId;
            InstanceId = instanceId;
        }

        internal static AssetReference Parse(BinaryReader reader)
        {
            return new AssetReference(
                reader.ReadUInt32(),
                reader.ReadUInt32());
        }
    }

    public sealed class AssetEntry
    {
        public uint TypeId { get; private set; }
        public uint InstanceId { get; private set; }
        public uint TypeHash { get; private set; }
        public uint InstanceHash { get; private set; }
        public uint AssetReferenceOffset { get; private set; }
        public uint AssetReferenceCount { get; private set; }
        public uint NameOffset { get; private set; }
        public uint SourceFileNameOffset { get; private set; }
        public uint InstanceDataSize { get; private set; }
        public uint RelocationDataSize { get; private set; }
        public uint ImportsDataSize { get; private set; }

        public bool IsTokenized { get; private set; }

        internal static AssetEntry Parse(BinaryReader reader, ushort manifestVersion)
        {
            var result = new AssetEntry
            {
                TypeId = reader.ReadUInt32(),
                InstanceId = reader.ReadUInt32(),
                TypeHash = reader.ReadUInt32(),
                InstanceHash = reader.ReadUInt32(),
                AssetReferenceOffset = reader.ReadUInt32(),
                AssetReferenceCount = reader.ReadUInt32(),
                NameOffset = reader.ReadUInt32(),
                SourceFileNameOffset = reader.ReadUInt32(),
                InstanceDataSize = reader.ReadUInt32(),
                RelocationDataSize = reader.ReadUInt32(),
                ImportsDataSize = reader.ReadUInt32(),
            };

            if (manifestVersion >= 6)
            {
                result.IsTokenized = reader.ReadBooleanUInt32Checked();
            }

            return result;
        }
    }

    public sealed class ManifestHeader
    {
        public bool IsBigEndian { get; private set; }
        public bool IsLinked { get; private set; }

        public ushort Version { get; private set; }

        public uint StreamChecksum { get; private set; }
        public uint AllTypesHash { get; private set; }
        public uint AssetCount { get; private set; }

        public uint TotalInstanceDataSize { get; private set; }
        public uint MaxInstanceChunkSize { get; private set; }

        public uint MaxRelocationChunkSize { get; private set; }
        public uint MaxImportsChunkSize { get; private set; }
        public uint AssetReferenceBufferSize { get; private set; }
        public uint ReferencedManifestNameBufferSize { get; private set; }
        public uint AssetNameBufferSize { get; private set; }
        public uint SourceFileNameBufferSize { get; private set; }

        internal static ManifestHeader Parse(BinaryReader reader)
        {
            var result = new ManifestHeader();

            var testValue = reader.ReadUInt32();
            if (testValue == 0)
            {
                result.Version = reader.ReadUInt16();

                if (result.Version != 7)
                {
                    throw new InvalidDataException();
                }

                result.IsBigEndian = reader.ReadBooleanChecked();
                result.IsLinked = reader.ReadBooleanChecked();
            }
            else
            {
                reader.BaseStream.Seek(-4, SeekOrigin.Current);

                result.IsBigEndian = reader.ReadBooleanChecked();
                result.IsLinked = reader.ReadBooleanChecked();
                result.Version = reader.ReadUInt16();

                if (result.Version != 5 && result.Version != 6)
                {
                    throw new InvalidDataException();
                }
            }

            result.StreamChecksum = reader.ReadUInt32();
            result.AllTypesHash = reader.ReadUInt32();
            result.AssetCount = reader.ReadUInt32();
            result.TotalInstanceDataSize = reader.ReadUInt32();
            result.MaxInstanceChunkSize = reader.ReadUInt32();
            result.MaxRelocationChunkSize = reader.ReadUInt32();
            result.MaxImportsChunkSize = reader.ReadUInt32();
            result.AssetReferenceBufferSize = reader.ReadUInt32();
            result.ReferencedManifestNameBufferSize = reader.ReadUInt32();
            result.AssetNameBufferSize = reader.ReadUInt32();
            result.SourceFileNameBufferSize = reader.ReadUInt32();

            return result;
        }
    }

    public sealed class ManifestReference
    {
        public ManifestReferenceType ReferenceType { get; private set; }
        public string Path { get; private set; }

        internal static ManifestReference Parse(BinaryReader reader)
        {
            return new ManifestReference
            {
                ReferenceType = reader.ReadByteAsEnum<ManifestReferenceType>(),
                Path = reader.ReadNullTerminatedString()
            };
        }
    }

    public enum ManifestReferenceType : byte
    {
        Reference = 1,
        Patch = 2
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.Data.Utilities.Extensions;

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

                if (result.Header.Version != 5)
                {
                    throw new System.NotImplementedException();
                }

                result.Assets = new Asset[result.Header.AssetCount];
                for (var i = 0; i < result.Assets.Length; i++)
                {
                    result.Assets[i] = new Asset
                    {
                        Header = AssetEntry.Parse(reader)
                    };
                }

                result.AssetReferences = new AssetReference[result.Header.AssetReferenceBufferSize / AssetReference.SizeInBytes];
                for (var i = 0; i < result.AssetReferences.Length; i++)
                {
                    result.AssetReferences[i] = AssetReference.Parse(reader);
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

                var instanceDataOffset = 4u;
                var relocationDataOffset = 4u;
                var importsDataOffset = 4u;
                foreach (var asset in result.Assets)
                {
                    asset.InstanceDataOffset = instanceDataOffset;
                    asset.RelocationDataOffset = relocationDataOffset;
                    asset.ImportsDataOffset = importsDataOffset;

                    instanceDataOffset += asset.Header.InstanceDataSize;
                    relocationDataOffset += asset.Header.RelocationDataSize;
                    importsDataOffset += asset.Header.ImportsDataSize;
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

        public string Name { get; internal set; }
        public string SourceFileName { get; internal set; }

        public uint InstanceDataOffset { get; internal set; }
        public uint RelocationDataOffset { get; internal set; }
        public uint ImportsDataOffset { get; internal set; }

        public byte[] InstanceData { get; internal set; }
    }

    public sealed class AssetReference
    {
        internal const int SizeInBytes = sizeof(uint) * 2;

        public uint TypeId { get; private set; }
        public uint InstanceId { get; private set; }

        internal static AssetReference Parse(BinaryReader reader)
        {
            return new AssetReference
            {
                TypeId = reader.ReadUInt32(),
                InstanceId = reader.ReadUInt32()
            };
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

        internal static AssetEntry Parse(BinaryReader reader)
        {
            return new AssetEntry
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

            result.IsBigEndian = reader.ReadBooleanChecked();
            result.IsLinked = reader.ReadBooleanChecked();
            result.Version = reader.ReadUInt16();

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

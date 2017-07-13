using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenZH.Data.Map
{
    public sealed class AssetNameCollection
    {
        private readonly Dictionary<uint, string> _assetIndexToName;
        private readonly Dictionary<string, uint> _assetNameToIndex;

        public AssetNameCollection()
        {
            _assetIndexToName = new Dictionary<uint, string>();
            _assetNameToIndex = new Dictionary<string, uint>();
        }

        public static AssetNameCollection Parse(BinaryReader reader)
        {
            var numAssetStrings = reader.ReadUInt32();

            var result = new AssetNameCollection();

            for (var i = numAssetStrings; i >= 1; i--)
            {
                var assetName = reader.ReadString();
                var assetIndex = reader.ReadUInt32();
                if (assetIndex != i)
                {
                    throw new InvalidDataException();
                }
                result.AddAssetName(assetIndex, assetName);
            }

            return result;
        }

        private void AddAssetName(uint index, string name)
        {
            _assetIndexToName[index] = name;
            _assetNameToIndex[name] = index;
        }

        public string GetAssetName(uint assetIndex)
        {
            if (_assetIndexToName.TryGetValue(assetIndex, out var assetName))
                return assetName;

            throw new ArgumentException($"Could not find name for asset index {assetIndex}.");
        }

        public uint GetOrCreateAssetIndex(string assetName)
        {
            if (_assetNameToIndex.TryGetValue(assetName, out var assetIndex))
                return assetIndex;

            assetIndex = (uint) _assetIndexToName.Count + 1;
            AddAssetName(assetIndex, assetName);

            return assetIndex;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) _assetIndexToName.Count);

            foreach (var assetIndex in _assetIndexToName.Keys.OrderByDescending(x => x))
            {
                var assetName = _assetIndexToName[assetIndex];

                writer.Write(assetName);
                writer.Write(assetIndex);
            }
        }
    }
}

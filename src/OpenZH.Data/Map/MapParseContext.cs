using System;
using System.Collections.Generic;

namespace OpenZH.Data.Map
{
    public sealed class MapParseContext
    {
        private readonly Stack<AssetStackEntry> _assetParsingStack;
        private readonly Dictionary<uint, string> _assetNames;

        public MapFile MapFile { get; }

        public MapParseContext(Dictionary<uint, string> assetNames, MapFile mapFile)
        {
            _assetParsingStack = new Stack<AssetStackEntry>();
            _assetNames = assetNames;
            MapFile = mapFile;
        }

        public string GetAssetName(uint assetIndex)
        {
            if (_assetNames.TryGetValue(assetIndex, out var assetName))
                return assetName;

            throw new ArgumentException($"Could not find name for asset index {assetIndex}.");
        }

        public long CurrentEndPosition => _assetParsingStack.Peek().EndPosition;

        public void PushAsset(string assetName, long endPosition)
        {
            _assetParsingStack.Push(new AssetStackEntry
            {
                AssetName = assetName,
                EndPosition = endPosition
            });
        }

        public void PopAsset()
        {
            _assetParsingStack.Pop();
        }

        private struct AssetStackEntry
        {
            public string AssetName;
            public long EndPosition;
        }
    }
}

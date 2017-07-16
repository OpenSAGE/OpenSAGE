using System.Collections.Generic;

namespace OpenZH.Data.Map
{
    public sealed class MapParseContext
    {
        private readonly Stack<AssetStackEntry> _assetParsingStack;
        private readonly AssetNameCollection _assetNames;

        public MapParseContext(AssetNameCollection assetNames)
        {
            _assetParsingStack = new Stack<AssetStackEntry>();
            _assetNames = assetNames;
        }

        public string GetAssetName(uint assetIndex) => _assetNames.GetAssetName(assetIndex);

        public long CurrentEndPosition => _assetParsingStack.Peek().EndPosition;

        public void PushAsset(string assetType, long endPosition)
        {
            _assetParsingStack.Push(new AssetStackEntry
            {
                AssetType = assetType,
                EndPosition = endPosition
            });
        }

        public void PopAsset()
        {
            _assetParsingStack.Pop();
        }

        private struct AssetStackEntry
        {
            public string AssetType;
            public long EndPosition;
        }
    }
}

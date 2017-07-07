using System.Collections.Generic;

namespace OpenZH.Data.Map
{
    public sealed class MapParseContext
    {
        private readonly Stack<long> _assetParsingStack;

        public Dictionary<uint, string> AssetNames { get; }
        public MapFile MapFile { get; }

        public MapParseContext(Dictionary<uint, string> assetNames, MapFile mapFile)
        {
            _assetParsingStack = new Stack<long>();
            AssetNames = assetNames;
            MapFile = mapFile;
        }

        public long CurrentEndPosition => _assetParsingStack.Peek();

        public void PushAsset(long endPosition)
        {
            _assetParsingStack.Push(endPosition);
        }

        public void PopAsset()
        {
            _assetParsingStack.Pop();
        }
    }
}

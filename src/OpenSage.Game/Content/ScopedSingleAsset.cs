using System.Collections.Generic;

namespace OpenSage.Content
{
    public sealed class ScopedSingleAsset<TAsset> : IScopedSingleAssetStorage
        where TAsset : BaseSingletonAsset, new()
    {
        private readonly Stack<TAsset> _assetStack;

        public TAsset Current
        {
            get => _assetStack.Peek();
            internal set
            {
                _assetStack.Pop().Dispose();
                _assetStack.Push(value);
            }
        }

        BaseAsset IScopedSingleAssetStorage.Current
        {
            get => Current;
            set => Current = (TAsset) value;
        }

        internal ScopedSingleAsset()
        {
            _assetStack = new Stack<TAsset>();
        }

        void IScopedAssetStorage.PushScope()
        {
            TAsset toPush;
            if (_assetStack.Count > 0)
            {
                var current = _assetStack.Peek();
                toPush = (TAsset) current.DeepClone();
            }
            else
            {
                toPush = new TAsset();
            }
            _assetStack.Push(toPush);
        }

        void IScopedAssetStorage.PopScope()
        {
            _assetStack.Pop().Dispose();
        }

        IEnumerable<BaseAsset> IScopedAssetStorage.GetAssets()
        {
            yield return _assetStack.Peek();
        }
    }
}

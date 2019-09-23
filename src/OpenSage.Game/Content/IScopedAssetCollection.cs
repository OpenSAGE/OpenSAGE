using System.Collections;

namespace OpenSage.Content
{
    internal interface IScopedAssetCollection : IEnumerable
    {
        void PushScope();
        void PopScope();
    }
}

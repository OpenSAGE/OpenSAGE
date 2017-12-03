using LL.Graphics3D;
using OpenSage.Data;

namespace OpenSage.Content
{
    internal abstract class ContentLoader<T> : ContentLoader
    {
        public sealed override object Load(FileSystemEntry entry, ContentManager contentManager)
        {
            return LoadEntry(entry, contentManager);
        }

        protected abstract T LoadEntry(FileSystemEntry entry, ContentManager contentManager);
    }
}

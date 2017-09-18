using LLGfx;
using OpenSage.Data;

namespace OpenSage.Content
{
    internal abstract class ContentLoader<T> : ContentLoader
    {
        public sealed override object Load(FileSystemEntry entry, ContentManager contentManager, ResourceUploadBatch uploadBatch)
        {
            return LoadEntry(entry, contentManager, uploadBatch);
        }

        protected abstract T LoadEntry(FileSystemEntry entry, ContentManager contentManager, ResourceUploadBatch uploadBatch);
    }
}

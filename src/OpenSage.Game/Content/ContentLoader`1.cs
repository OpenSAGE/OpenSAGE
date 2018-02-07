using OpenSage.Data;

namespace OpenSage.Content
{
    internal abstract class ContentLoader<T> : ContentLoader
    {
        public sealed override object Load(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            return LoadEntry(entry, contentManager, game, loadOptions);
        }

        protected abstract T LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions);
    }
}

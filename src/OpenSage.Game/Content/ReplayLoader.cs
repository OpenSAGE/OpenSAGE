using OpenSage.Data;
using OpenSage.Data.Rep;

namespace OpenSage.Content
{
    internal sealed class ReplayLoader : ContentLoader<ReplayFile>
    {
        protected override ReplayFile LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            return ReplayFile.FromFileSystemEntry(entry);
        }
    }
}

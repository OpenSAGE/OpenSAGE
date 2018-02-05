using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Gui.Apt;

namespace OpenSage.Content
{
    internal sealed class AptLoader : ContentLoader<AptWindow>
    {
        protected override AptWindow LoadEntry(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions)
        {
            var aptFile = AptFile.FromFileSystemEntry(entry);
            return AddDisposable(new AptWindow(contentManager, aptFile));
        }
    }
}

using System;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Gui.Apt;

namespace OpenSage.Content
{
    internal sealed class AptLoader : ContentLoader<AptComponent>
    {
        public AptLoader(ContentManager contentManager)
        {
        }

        protected override AptComponent LoadEntry(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions)
        {
            //load the corresponding .dat file
            var aptFile = AptFile.FromFileSystemEntry(entry);

            return new AptComponent() { Movie = aptFile };
        }
    }
}

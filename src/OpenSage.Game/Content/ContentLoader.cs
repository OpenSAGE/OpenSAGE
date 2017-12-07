using System.Collections.Generic;
using LL.Graphics3D;
using OpenSage.Data;

namespace OpenSage.Content
{
    internal abstract class ContentLoader : GraphicsObject
    {
        public virtual object PlaceholderValue => null;

        public virtual IEnumerable<string> GetPossibleFilePaths(string filePath)
        {
            yield return filePath;
        }

        public abstract object Load(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions);
    }
}

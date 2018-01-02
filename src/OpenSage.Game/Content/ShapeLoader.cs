using System;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Gui.Apt;

namespace OpenSage.Content
{
    internal sealed class ShapeLoader : ContentLoader<ShapeComponent>
    {
        public ShapeLoader(ContentManager contentManager)
        {
        }

        protected override ShapeComponent LoadEntry(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions)
        {
            //load the corresponding .dat file
            var movieName = entry.FilePath.Split('/')[0].Split('_')[0];
            var datPath = movieName + ".dat";
            var datEntry = contentManager.FileSystem.GetFile(datPath);
            var imageMap = ImageMap.FromFileSystemEntry(datEntry);

            var shape = Geometry.FromFileSystemEntry(entry);

            var component = new ShapeComponent() { Shape = shape , MovieName = movieName};
            component.Initialize(contentManager, imageMap);

            return component;
        }
    }
}

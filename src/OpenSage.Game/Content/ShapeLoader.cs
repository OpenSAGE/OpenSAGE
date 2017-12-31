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
            switch (contentManager.SageGame)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    contentManager.IniDataContext.LoadIniFile(@"Data\English\HeaderTemplate.ini");
                    break;

                case SageGame.BattleForMiddleEarth:
                    contentManager.IniDataContext.LoadIniFile(@"lang\english\headertemplate.ini");
                    break;

                case SageGame.BattleForMiddleEarthII:
                    contentManager.IniDataContext.LoadIniFile(@"headertemplate.ini");
                    break;
            }

            contentManager.IniDataContext.LoadIniFiles(@"Data\INI\MappedImages\HandCreated\");
            contentManager.IniDataContext.LoadIniFiles(@"Data\INI\MappedImages\TextureSize_512\");
        }

        protected override ShapeComponent LoadEntry(FileSystemEntry entry, ContentManager contentManager, LoadOptions loadOptions)
        {
            //load the corresponding .dat file
            var datPath = entry.FilePath.Split('/')[0].Split('_')[0]+".dat";
            var datEntry = contentManager.FileSystem.GetFile(datPath);
            var imageMap = ImageMap.FromFileSystemEntry(datEntry);

            var shape = Geometry.FromFileSystemEntry(entry);

            var component = new ShapeComponent() { Shape = shape };
            component.Initialize(contentManager);

            return component;
        }
    }
}

using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Csf;

namespace OpenSage.Content
{
    public sealed class TranslationManager
    {
        private readonly CsfFile _csfFile;

        public TranslationManager(FileSystem fileSystem,SageGame game)
        {
            FileSystemEntry csfEntry = null;

            switch (game)
            {
                case SageGame.CncGenerals:
                    csfEntry = fileSystem.GetFile(@"Data\English\generals.csf");
                    break;
                case SageGame.CncGeneralsZeroHour:
                    break;
                case SageGame.BattleForMiddleEarth:
                    csfEntry = fileSystem.GetFile(@"lotr.csf");
                    break;
                case SageGame.BattleForMiddleEarthII:
                    csfEntry = fileSystem.GetFile(@"lotr.csf");
                    break;
                case SageGame.Cnc3:
                    break;
                case SageGame.Cnc3KanesWrath:
                    break;
                case SageGame.Ra3:
                    break;
                case SageGame.Ra3Uprising:
                    break;
                case SageGame.Cnc4:
                    break;
            }

            if (csfEntry != null)
            {
                // TODO: Each game probably has its own path for this file.
                _csfFile = CsfFile.FromFileSystemEntry(csfEntry);
            }
        }

        public string Lookup(string key)
        {
            var label = _csfFile?.Labels.FirstOrDefault(x => x.Name == key);
            return label?.Strings[0].Value ?? key;
        }
    }
}

using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Csf;

namespace OpenSage.Content
{
    public sealed class TranslationManager
    {
        private readonly CsfFile _csfFile;

        public TranslationManager(FileSystem fileSystem, SageGame game, string language)
        {
            FileSystemEntry csfEntry = null;

            switch (game)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    csfEntry = fileSystem.GetFile($@"Data\{language}\generals.csf");
                    break;
                case SageGame.Bfme:
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
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

        public string Format(string key, params object[] args)
        {
            var label = _csfFile?.Labels.FirstOrDefault(x => x.Name == key);
            var str = label?.Strings[0].Value ?? key;

            str = str.Replace("%d", "{0}");

            return string.Format(str, args);
        }
    }
}

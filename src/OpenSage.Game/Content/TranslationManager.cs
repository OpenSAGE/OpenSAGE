using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Csf;

namespace OpenSage.Content
{
    public sealed class TranslationManager
    {
        private readonly List<CsfFile> _csfFiles;

        public IReadOnlyList<CsfLabel> Labels { get; }

        public TranslationManager(FileSystem fileSystem, SageGame game, string language)
        {
            _csfFiles = new List<CsfFile>();
            FileSystemEntry csfEntry = null;
            var currentFileSystem = fileSystem;

            while (currentFileSystem != null)
            {

                switch (game)
                {
                    case SageGame.CncGenerals:
                    case SageGame.CncGeneralsZeroHour:
                        csfEntry = fileSystem.GetFile($@"Data\{language}\generals.csf");
                        break;
                    case SageGame.Bfme:
                        csfEntry = fileSystem.GetFile($@"Lang\{language}\lotr.csf");
                        break;
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
                    _csfFiles.Add(CsfFile.FromFileSystemEntry(csfEntry));
                }

                currentFileSystem = currentFileSystem.NextFileSystem;
            }

            Labels = _csfFiles.SelectMany(x => x.Labels).ToList();
        }

        public string Lookup(string key)
        {
            foreach (var csfFile in _csfFiles)
            {
                var label = csfFile?.Labels.FirstOrDefault(x => x.Name.ToLowerInvariant() == key?.ToLowerInvariant());

                if (label != null)
                {
                    return label.Strings[0].Value;
                }
            }

            return key;
        }

        public string Format(string key, params object[] args)
        {
            foreach (var csfFile in _csfFiles)
            {
                var label = csfFile?.Labels.FirstOrDefault(x => x.Name.ToLowerInvariant() == key?.ToLowerInvariant());

                if (label != null)
                {
                    var str = label.Strings[0].Value;
                    str = str.Replace("%d", "{0}");

                    return string.Format(str, args);
                }
            }

            return key;
        }
    }
}

using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Csf;

namespace OpenSage.Content
{
    public sealed class TranslationManager
    {
        private readonly CsfFile _csfFile;

        public TranslationManager(FileSystem fileSystem)
        {
            var csfEntry = fileSystem.GetFile(@"Data\English\generals.csf");
            if (csfEntry != null)
            {
                // TODO: Each game probably has its own path for this file.
                _csfFile = CsfFile.FromFileSystemEntry(csfEntry);
            }
        }

        public string Lookup(string key)
        {
            var label = _csfFile.Labels.FirstOrDefault(x => x.Name == key);
            return label?.Strings[0].Value ?? key;
        }
    }
}

using OpenSage.Audio;
using OpenSage.Data;
using OpenSage.Data.Wav;

namespace OpenSage.Content
{
    internal sealed class WavLoader : ContentLoader<WavFile>
    {
        protected override WavFile LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            var wavFile = WavFile.FromFileSystemEntry(entry);
            return wavFile;
        }
    }
}

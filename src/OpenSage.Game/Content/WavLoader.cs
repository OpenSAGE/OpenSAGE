using OpenSage.Audio;
using OpenSage.Data;
using OpenSage.Data.Wav;

namespace OpenSage.Content
{
    internal sealed class WavLoader : ContentLoader<AudioBuffer>
    {
        protected override AudioBuffer LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            var aptFile = WavFile.FromFileSystemEntry(entry);
            return AddDisposable(new AudioBuffer(contentManager, aptFile));
        }
    }
}

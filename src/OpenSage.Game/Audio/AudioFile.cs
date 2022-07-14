using OpenSage.IO;

namespace OpenSage.Audio
{
    public sealed class AudioFile : BaseAsset
    {
        internal static AudioFile FromFileSystemEntry(FileSystemEntry entry, string name)
        {
            var result = new AudioFile
            {
                Entry = entry
            };
            result.SetNameAndInstanceId("AudioFile", name);
            return result;
        }

        public FileSystemEntry Entry { get; private set; }
    }
}

using System.IO;
using OpenSage.Audio;
using OpenSage.IO;

namespace OpenSage.Content.Loaders
{
    internal sealed class OnDemandAudioFileLoader : IOnDemandAssetLoader<AudioFile>
    {
        public AudioFile Load(string key, AssetLoadContext context)
        {
            FileSystemEntry entry = null;

            // audio events
            if (string.IsNullOrEmpty(Path.GetExtension(key)))
            {
                var audioSettings = context.AssetStore.AudioSettings.Current;

                var soundFileName = $"{key}.{audioSettings.SoundsExtension}";

                var localisedAudioRoot = Path.Combine(audioSettings.AudioRoot, audioSettings.SoundsFolder, context.Language);
                var audioRoot = Path.Combine(audioSettings.AudioRoot, audioSettings.SoundsFolder);

                foreach (var rootPath in new[] { localisedAudioRoot, audioRoot })
                {
                    var fullPath = Path.Combine(rootPath, soundFileName);
                    entry = context.FileSystem.GetFile(fullPath);
                    if (entry != null)
                    {
                        break;
                    }
                }
            }
            else // music tracks
            {
                entry = context.FileSystem.GetFile(Path.Combine(@"Data\Audio\Tracks", key));
            }

            if (entry == null)
            {
                return null;
            }

            return AudioFile.FromFileSystemEntry(entry, key);
        }
    }
}

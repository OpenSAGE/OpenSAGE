using System.IO;
using OpenSage.Audio;
using OpenSage.Data;

namespace OpenSage.Content.Loaders
{
    internal sealed class OnDemandAudioFileLoader : IOnDemandAssetLoader<string, AudioFile>
    {
        public AudioFile Load(string key, AssetLoadContext context)
        {
            var audioSettings = context.AssetStore.AudioSettings;

            var soundFileName = $"{key}.{audioSettings.SoundsExtension}";

            var localisedAudioRoot = Path.Combine(audioSettings.AudioRoot, audioSettings.SoundsFolder, context.Language);
            var audioRoot = Path.Combine(audioSettings.AudioRoot, audioSettings.SoundsFolder);

            FileSystemEntry entry = null;
            foreach (var rootPath in new[] { localisedAudioRoot, audioRoot })
            {
                var fullPath = Path.Combine(rootPath, soundFileName);
                entry = context.FileSystem.GetFile(fullPath);
                if (entry != null)
                {
                    break;
                }
            }

            if (entry == null)
            {
                return null;
            }

            return new AudioFile
            {
                Name = key,
                Entry = entry
            };
        }
    }
}

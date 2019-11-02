using OpenSage.Audio;
using OpenSage.Data.IO;

namespace OpenSage.Content.Loaders
{
    internal sealed class OnDemandAudioFileLoader : IOnDemandAssetLoader<AudioFile>
    {
        public AudioFile Load(string key, AssetLoadContext context)
        {
            var audioSettings = context.AssetStore.AudioSettings.Current;

            var soundFileName = $"{key}.{audioSettings.SoundsExtension}";

            var localisedAudioRoot = FileSystem.Combine("/game", audioSettings.AudioRoot, audioSettings.SoundsFolder, context.Language);
            var audioRoot = FileSystem.Combine("/game", audioSettings.AudioRoot, audioSettings.SoundsFolder);

            string url = null;
            foreach (var rootPath in new[] { localisedAudioRoot, audioRoot })
            {
                var fullPath = FileSystem.Combine(rootPath, soundFileName);
                if (FileSystem.FileExists(fullPath))
                {
                    url = fullPath;
                    break;
                }
            }

            if (url is null)
            {
                return null;
            }

            return AudioFile.FromUrl(url, key);
        }
    }
}

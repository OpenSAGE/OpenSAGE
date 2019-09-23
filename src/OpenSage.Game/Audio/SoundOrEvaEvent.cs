using System.IO;
using OpenSage.Content;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    public abstract class SoundOrEvaEvent
    {
        public LazyAssetReference<BaseAudioEventInfo> Sound { get; internal set; }
        public string EvaEvent { get; private set; }

        private protected static void ParseAsset<T>(BinaryReader reader, T asset, AssetImportCollection imports)
            where T : SoundOrEvaEvent
        {
            asset.Sound = reader.ReadOptionalClassTypedValueAtOffset(() => new LazyAssetReference<BaseAudioEventInfo>(imports.GetImportedData<BaseAudioEventInfo>(reader)));
            asset.EvaEvent = reader.ReadOptionalClassTypedValueAtOffset(() => reader.ReadUInt32PrefixedAsciiString());
        }
    }
}

using OpenSage.Content;

namespace OpenSage.Audio
{
    public abstract class SoundOrEvaEvent
    {
        public LazyAssetReference<BaseAudioEventInfo> Sound { get; internal set; }
        public string EvaEvent { get; private set; }
    }
}

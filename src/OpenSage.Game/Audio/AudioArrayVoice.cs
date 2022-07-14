using System.Collections.Generic;

namespace OpenSage.Audio
{
    public sealed class AudioArrayVoice
    {
        public List<AudioVoiceEntry> AudioEntries { get; private set; } = new List<AudioVoiceEntry>();
    }
}

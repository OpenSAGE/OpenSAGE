using System;
using System.Collections.Generic;
using System.Text;
using OpenAL;

namespace OpenSage.Audio
{
    public sealed class AudioSource
    {
        private uint _handle;
        private bool _looping = false;
        private AudioBuffer _buffer;

        public AudioSource()
        {
            AL10.alGenSources(1, out _handle);
            AudioSystem.alCheckError();
        }

        public AudioSource(AudioBuffer buffer) : this()
        {
            AL10.alSourcei(_handle, AL10.AL_BUFFER, (int)buffer.Handle);
            _buffer = buffer;
        }

        ~AudioSource()
        {
            Stop();
            AL10.alDeleteSources(1, ref _handle);
        }

        public bool Looping
        {
            get { return _looping; }
            set { _looping = value;
                AL10.alSourcei(_handle, AL10.AL_LOOPING, Convert.ToInt32(_looping));
            }
        }

        public void Stop()
        {
            AL10.alSourceStop(_handle);
        }

        public void Play()
        {
            AL10.alSourcePlay(_handle);
        }
    }
}

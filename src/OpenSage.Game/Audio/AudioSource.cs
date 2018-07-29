using System;
using System.Collections.Generic;
using System.Text;
using OpenAL;

namespace OpenSage.Audio
{
    public sealed class AudioSource : DisposableBase
    {
        private float _volume;
        private uint _handle;
        private bool _looping = false;
        private TimeSpan _duration;

        public AudioSource()
        {
            AL10.alGenSources(1, out _handle);
            AudioSystem.alCheckError();
        }

        public AudioSource(AudioBuffer buffer) : this()
        {
            AL10.alSourcei(_handle, AL10.AL_BUFFER, (int)buffer.Handle);
            _duration = buffer.GetDuration();
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (_handle > 0)
            {
                Stop();
                AL10.alDeleteSources(1, ref _handle);              
                _handle = 0;
            }
            base.Dispose(disposeManagedResources);
        }

        public bool Looping
        {
            get { return _looping; }
            set { _looping = value;
                AL10.alSourcei(_handle, AL10.AL_LOOPING, Convert.ToInt32(_looping));
            }
        }

        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                AL10.alSourcef(_handle, AL10.AL_GAIN, value / 100.0f);
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

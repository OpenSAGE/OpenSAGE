using System;
using System.Collections.Generic;
using System.Text;
using OpenSage.Content;
using OpenSage.Data.Wav;
using OpenAL;

namespace OpenSage.Audio
{
    public sealed class AudioBuffer : DisposableBase
    {
        private uint _handle;
        private List<AudioSource> _usageList;

        public List<AudioSource> UsageList => _usageList;
        public uint Handle => _handle;

        public AudioBuffer()
        {
            AL10.alGenBuffers(1, out _handle);
            AudioSystem.alCheckError();
            _usageList = new List<AudioSource>();
        }

        public AudioBuffer(ContentManager contentManager, WavFile wavFile) : this()
        {
            int format = 0;

            switch (wavFile.Channels)
            {
                case 1:
                    {
                        switch (wavFile.BitsPerSample)
                        {
                            case 8:
                                format = AL10.AL_FORMAT_MONO8;
                                break;
                            case 16:
                                format = AL10.AL_FORMAT_MONO16;
                                break;
                            default:
                                throw new NotSupportedException("Invalid audio format!");
                        }
                    }
                    break;
                case 2:
                    {
                        switch (wavFile.BitsPerSample)
                        {
                            case 8:
                                format = AL10.AL_FORMAT_STEREO8;
                                break;
                            case 16:
                                format = AL10.AL_FORMAT_STEREO16;
                                break;
                            default:
                                throw new NotSupportedException("Invalid audio format!");
                        }
                    }
                    break;
            }


            AL10.alBufferData(_handle, format, wavFile.Buffer, wavFile.Size, wavFile.Fequency);
            AudioSystem.alCheckError();
        }

        public TimeSpan Duration
        {
            get
            {
                int sizeInBytes;
                int channels;
                int bits;
                int lengthInSamples;
                AL10.alGetBufferi(_handle, AL10.AL_SIZE, out sizeInBytes);
                AL10.alGetBufferi(_handle, AL10.AL_CHANNELS, out channels);
                AL10.alGetBufferi(_handle, AL10.AL_BITS, out bits);

                lengthInSamples = sizeInBytes * 8 / (channels * bits);

                int frequency;

                AL10.alGetBufferi(_handle, AL10.AL_FREQUENCY, out frequency);
                return TimeSpan.FromSeconds(lengthInSamples / (float) frequency);
            }         
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            if (_handle > 0)
            {
                for(int i=0;i<_usageList.Count;++i)
                {
                    _usageList[i].Dispose();
                }

                AL10.alDeleteBuffers(1, ref _handle);
                AudioSystem.alCheckError();
                _handle = 0;
                
                base.Dispose(disposeManagedResources);
            }
        }
    }
}

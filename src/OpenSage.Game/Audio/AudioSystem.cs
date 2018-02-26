using System;
using System.Collections.Generic;
using System.Text;
using OpenAL;

namespace OpenSage.Audio
{
    public sealed class AudioSystem : GameSystem
    {
        private IntPtr _device;
        private IntPtr _context;

        private List<AudioSource> _sources;

        internal static void alCheckError()
        {
            int error;
            error = AL10.alGetError();

            if (error != AL10.AL_NO_ERROR)
            {
                throw new InvalidOperationException("AL error!");
            }
        }

        internal void alcCheckError()
        {
            int error;
            error = ALC10.alcGetError(_device);

            if (error != ALC10.ALC_NO_ERROR)
            {
                throw new InvalidOperationException("ALC error!");
            }
        }

        public AudioSystem(Game game) : base(game)
        {
            _device = ALC10.alcOpenDevice("");
            alcCheckError();

            _context = ALC10.alcCreateContext(_device, null);
            alcCheckError();

            ALC10.alcMakeContextCurrent(_context);
            alcCheckError();

            _sources = new List<AudioSource>();
        }

        ~AudioSystem()
        {
            ALC10.alcMakeContextCurrent(IntPtr.Zero);
            ALC10.alcDestroyContext(_context);
            ALC10.alcCloseDevice(_device);
        }

        public AudioSource AddSource(AudioBuffer buffer,bool loop=false)
        {
            var source = new AudioSource(buffer);

            if(loop)
            {
                source.Looping = true;
            }

            _sources.Add(source);

            return source;
        }

        public void RemoveSource(AudioSource source)
        {
            source.Dispose();
            _sources.Remove(source);
        }
    }
}

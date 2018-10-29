using System;
using System.Collections.Generic;
using System.IO;
using OpenAL;
using OpenSage.Data.Ini;
using OpenSage.Data.Wav;

namespace OpenSage.Audio
{
    public sealed class AudioSystem : GameSystem
    {
        private IntPtr _device;
        private IntPtr _context;

        private List<AudioSource> _sources;
        private Dictionary<string, AudioBuffer> _files;
        private AudioSettings _settings;

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
            _files = new Dictionary<string, AudioBuffer>();

            switch (game.ContentManager.SageGame)
            {
                case SageGame.Ra3:
                case SageGame.Ra3Uprising:
                case SageGame.Cnc4:
                    // TODO
                    break;

                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                case SageGame.Bfme:
                //TODO: complete audiosettings for BFME
                //case SageGame.Bfme2:
                //case SageGame.Bfme2Rotwk:
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\AudioSettings.ini");
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\SoundEffects.ini");
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\MiscAudio.ini");
                    break;
                default:

                    break;
            }

            _settings = game.ContentManager.IniDataContext.AudioSettings;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);
            _sources.Clear();
            _files.Clear();


            ALC10.alcMakeContextCurrent(IntPtr.Zero);
            ALC10.alcDestroyContext(_context);
            ALC10.alcCloseDevice(_device);
          
        }

        public AudioSource GetFile(string fileName,bool loop=false)
        {
            AudioBuffer buffer = null;
            if (!_files.ContainsKey(fileName))
            {
                var file = Game.ContentManager.Load<WavFile>(fileName);
                buffer = AddDisposable(new AudioBuffer(file));
                _files[fileName] = buffer;
            }
            else
            {
                buffer = _files[fileName];
            }

            var source = AddDisposable(new AudioSource(buffer));

            if(loop)
            {
                source.Looping = true;
            }

            _sources.Add(source);

            return source;
        }

        public void PlaySound(string soundName)
        {
            AudioBuffer buffer = null;

            AudioEvent ev = Game.ContentManager.IniDataContext.AudioEvents.Find(x => x.Name == soundName);
            string[] paths = { _settings.AudioRoot, _settings.SoundsFolder, ev.Sounds[0] };
            string soundPath = Path.Combine(paths);
            soundPath = Path.ChangeExtension(soundPath, _settings.SoundsExtension);

            if (!_files.ContainsKey(soundPath))
            {
                var file = Game.ContentManager.Load<WavFile>(soundPath);
                buffer = AddDisposable(new AudioBuffer(file));
                _files[soundPath] = buffer;
            }
            else
            {
                buffer = _files[soundPath];
            }

            var source = AddDisposable(new AudioSource(buffer));
                  
            if (ev.Control.HasFlag(AudioControlFlags.Loop))
            {
                source.Looping = true;
            }

            //set the volume of the sound
            source.Volume = ev.Volume;

            _sources.Add(source);
            source.Play();
        }
    }
}

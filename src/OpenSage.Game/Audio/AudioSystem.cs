using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini;
using OpenSage.Data.Wav;
using SharpAudio;

namespace OpenSage.Audio
{
    public sealed class AudioSystem : GameSystem
    {
        private IntPtr _device;
        private IntPtr _context;

        private List<AudioSource> _sources;
        private Dictionary<string, AudioBuffer> _cached;
        private AudioSettings _settings;
        private AudioEngine _engine;
        
        public AudioSystem(Game game) : base(game)
        {
            _engine = AudioEngine.CreateDefault();
            _sources = new List<AudioSource>();
            _cached = new Dictionary<string, AudioBuffer>();

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
            _cached.Clear();
          
            _engine.Dispose();
        }

        public AudioSource GetSound(string soundName, bool loop = false)
        {
            AudioBuffer buffer = null;
            if (!_cached.ContainsKey(soundName))
            {
                var file = Game.ContentManager.Load<WavFile>(soundName);

                buffer = AddDisposable(_engine.CreateBuffer());
                _cached[soundName] = buffer;
            }
            else
            {
                buffer = _cached[soundName];
            }

            var source = AddDisposable(_engine.CreateSource());
            source.QueryBuffer(buffer);

            if(loop)
            {
                //source.Looping = true;
            }

            _sources.Add(source);

            return source;
        }

        public void PlaySound(string soundName)
        {
            AudioEvent ev = Game.ContentManager.IniDataContext.AudioEvents.Find(x => x.Name == soundName);
            string[] paths = { _settings.AudioRoot, _settings.SoundsFolder, ev.Sounds[0] };
            string soundPath = Path.Combine(paths);
            soundPath = Path.ChangeExtension(soundPath, _settings.SoundsExtension);

            var source = GetSound(soundPath, ev.Control.HasFlag(AudioControlFlags.Loop));

            //set the volume of the sound
            source.Volume = ev.Volume;

            _sources.Add(source);
            source.Play();
        }
    }
}

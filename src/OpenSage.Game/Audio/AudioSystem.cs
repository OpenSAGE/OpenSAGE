using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini;
using SharpAudio;
using SharpAudio.Util;
using SharpAudio.Util.Wave;

namespace OpenSage.Audio
{
    public sealed class AudioSystem : GameSystem
    {
        private readonly List<AudioSource> _sources;
        private readonly Dictionary<string, AudioBuffer> _cached;
        private readonly AudioSettings _settings;
        private readonly AudioEngine _engine;
        
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
                //TODO: complete audio settings for BFME
                //case SageGame.Bfme2:
                //case SageGame.Bfme2Rotwk:
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\AudioSettings.ini");
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\SoundEffects.ini");
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\MiscAudio.ini");
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

        /// <summary>
        /// Opens a cached audio file. Usually used for small audio files (.wav)
        /// </summary>
        public AudioSource GetSound(string soundPath, bool loop = false)
        {
            AudioBuffer buffer;

            if (!_cached.ContainsKey(soundPath))
            {
                var entry = Game.ContentManager.FileSystem.GetFile(soundPath);
                var decoder = new WaveDecoder(entry.Open());
                byte[] data = null;
                decoder.GetSamples(ref data);

                buffer = AddDisposable(_engine.CreateBuffer());
                buffer.BufferData(data, decoder.Format);
     
                _cached[soundPath] = buffer;
            }
            else
            {
                buffer = _cached[soundPath];
            }

            var source = AddDisposable(_engine.CreateSource());
            source.QueryBuffer(buffer);
            // TODO: Implement looping

            _sources.Add(source);

            return source;
        }

        /// <summary>
        /// Open a a music/audio file that gets streamed.
        /// </summary>
        public SoundStream GetStream(string streamPath)
        {
            var entry = Game.ContentManager.FileSystem.GetFile(streamPath);

            return new SoundStream(entry.Open(), _engine);
        }

        public void PlayAudioEvent(string eventName)
        {
            if (!Game.ContentManager.IniDataContext.AudioEvents.TryGetValue(eventName, out var ev))
            {
                // TODO: Log a warning about a missing AudioEvent
                return;
            }

            // TODO: Remove these allocations.
            var soundFileName = ev.Sounds[0] + _settings.SoundsExtension;
            var soundPath = Path.Combine(_settings.AudioRoot, _settings.SoundsFolder, soundFileName);

            var source = GetSound(soundPath, ev.Control.HasFlag(AudioControlFlags.Loop));
            _sources.Add(source);

            // Divide the volume by 100, because SAGE uses the scale [0, 100] while SharpAudio uses [0, 1]
            source.Volume = ev.Volume / 100.0f;

            source.Play();
        }
    }
}

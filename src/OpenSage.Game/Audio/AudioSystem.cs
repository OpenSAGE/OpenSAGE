using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data;
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

        private readonly string _localisedAudioRoot;
        private readonly string _audioRoot;

        private readonly Random _random;

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
                case SageGame.Bfme2:
                //case SageGame.Bfme2Rotwk:
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\AudioSettings.ini");
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\SoundEffects.ini");
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\MiscAudio.ini");
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\Voice.ini");
                    break;
            }

            _settings = game.ContentManager.IniDataContext.AudioSettings;

            _localisedAudioRoot = Path.Combine(_settings.AudioRoot, _settings.SoundsFolder, Game.ContentManager.Language);
            _audioRoot = Path.Combine(_settings.AudioRoot, _settings.SoundsFolder);

            // TODO: Sync RNG seed from replay?
            _random = new Random();
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
        public AudioSource GetSound(FileSystemEntry entry, bool loop = false)
        {
            AudioBuffer buffer;

            if (!_cached.ContainsKey(entry.FilePath))
            {
                var decoder = new WaveDecoder(entry.Open());
                byte[] data = null;
                decoder.GetSamples(ref data);

                buffer = AddDisposable(_engine.CreateBuffer());
                buffer.BufferData(data, decoder.Format);
     
                _cached[entry.FilePath] = buffer;
            }
            else
            {
                buffer = _cached[entry.FilePath];
            }

            var source = AddDisposable(_engine.CreateSource());
            source.QueryBuffer(buffer);
            // TODO: Implement looping

            _sources.Add(source);

            return source;
        }

        private FileSystemEntry ResolveAudioEventPath(AudioEvent ev)
        {
            // TODO: Try to remove these allocations.

            // TOOD: Check control flag before choosing at random?
            var soundFileName = $"{ev.Sounds[_random.Next(ev.Sounds.Length)]}.{_settings.SoundsExtension}";
            var filePath = Path.Combine(ev.Type.Get(AudioTypeFlags.Voice) ? _localisedAudioRoot : _audioRoot, soundFileName);
            return Game.ContentManager.FileSystem.GetFile(filePath);
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

            var entry = ResolveAudioEventPath(ev);

            if (entry == null)
            {
                // TODO: Log a warning about a missing audio file.
                return;
            }

            var source = GetSound(entry, ev.Control.HasFlag(AudioControlFlags.Loop));
            _sources.Add(source);

            // Divide the volume by 100, because SAGE uses the scale [0, 100] while SharpAudio uses [0, 1]
            source.Volume = ev.Volume / 100.0f;

            source.Play();
        }
    }
}

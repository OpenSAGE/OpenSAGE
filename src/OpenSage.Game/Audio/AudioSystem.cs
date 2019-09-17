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

            _settings = game.AssetStore.AudioSettings;

            // TODO: AudioSettings should never be null.
            if (_settings != null)
            {
                _localisedAudioRoot = Path.Combine(_settings.AudioRoot, _settings.SoundsFolder, Game.ContentManager.Language);
                _audioRoot = Path.Combine(_settings.AudioRoot, _settings.SoundsFolder);
            }

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
            if(ev.Sounds.Length == 0)
            {
                return null;
            }

            // TOOD: Check control flag before choosing at random?
            var soundFileName = $"{ev.Sounds[_random.Next(ev.Sounds.Length)]}.{_settings.SoundsExtension}";
            var isLocalised = ev.Type.HasFlag(AudioTypeFlags.Voice);
            var filePath = Path.Combine(isLocalised ? _localisedAudioRoot : _audioRoot, soundFileName);
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

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void PlayAudioEvent(string eventName)
        {
            var audioEvent = Game.AssetStore.AudioEvents.GetByName(eventName);
            if (audioEvent == null)
            {
                logger.Warn($"Missing AudioEvent: {eventName}");
                return;
            }

            var entry = ResolveAudioEventPath(audioEvent);

            if (entry == null)
            {
                logger.Warn($"Missing Audio File: {audioEvent.Name}");
                return;
            }

            var source = GetSound(entry, audioEvent.Control.HasFlag(AudioControlFlags.Loop));
            _sources.Add(source);

            source.Volume = (float) audioEvent.Volume;

            source.Play();
        }
    }
}

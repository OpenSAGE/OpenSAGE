using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using SharpAudio;
using SharpAudio.Codec;
using SharpAudio.Codec.Wave;

namespace OpenSage.Audio
{
    public sealed class AudioSystem : GameSystem
    {
        private readonly List<AudioSource> _sources;
        private readonly Dictionary<string, AudioBuffer> _cached;
        private readonly AudioEngine _engine;
        private readonly AudioSettings _settings;
        private readonly Dictionary<AudioVolumeSlider, Submixer> _mixers;

        private readonly Random _random;

        public AudioSystem(Game game) : base(game)
        {
            _engine = AudioEngine.CreateDefault();
            _sources = new List<AudioSource>();
            _cached = new Dictionary<string, AudioBuffer>();
            _mixers = new Dictionary<AudioVolumeSlider, Submixer>();
            _settings = game.AssetStore.AudioSettings.Current;

            CreateSubmixers();

            // TODO: Sync RNG seed from replay?
            _random = new Random();
        }

        private void CreateSubmixers()
        {
            // Create all available mixers
            _mixers[AudioVolumeSlider.SoundFX] = _engine.CreateSubmixer();
            _mixers[AudioVolumeSlider.SoundFX].Volume = (float) _settings.DefaultSoundVolume;

            _mixers[AudioVolumeSlider.Music] = _engine.CreateSubmixer();
            _mixers[AudioVolumeSlider.Music].Volume = (float) _settings.DefaultMusicVolume;

            _mixers[AudioVolumeSlider.Ambient] = _engine.CreateSubmixer();
            _mixers[AudioVolumeSlider.Ambient].Volume = (float) _settings.DefaultAmbientVolume;

            _mixers[AudioVolumeSlider.Voice] = _engine.CreateSubmixer();
            _mixers[AudioVolumeSlider.Voice].Volume = (float) _settings.DefaultVoiceVolume;

            _mixers[AudioVolumeSlider.Movie] = _engine.CreateSubmixer();
            _mixers[AudioVolumeSlider.Movie].Volume = (float) _settings.DefaultMovieVolume;
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
        public AudioSource GetSound(FileSystemEntry entry,
            AudioVolumeSlider? vslider = AudioVolumeSlider.None, bool loop = false)
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

            var mixer = (vslider.HasValue && vslider.Value != AudioVolumeSlider.None) ?
                        _mixers[vslider.Value] : null;
            var source = AddDisposable(_engine.CreateSource(mixer));
            source.QueueBuffer(buffer);
            // TODO: Implement looping

            _sources.Add(source);

            return source;
        }

        private FileSystemEntry ResolveAudioEventEntry(AudioEvent ev)
        {
            if (ev.Sounds.Length == 0)
            {
                return null;
            }

            // TOOD: Check control flag before choosing at random.
            var sound = ev.Sounds[_random.Next(ev.Sounds.Length)];
            var audioFile = sound.AudioFile.Value;
            return audioFile.Entry;
        }

        /// <summary>
        /// Open a a music/audio file that gets streamed.
        /// </summary>
        public SoundStream GetStream(FileSystemEntry entry)
        {
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

            PlayAudioEvent(audioEvent);
        }

        public void PlayAudioEvent(BaseAudioEventInfo baseAudioEvent)
        {
            if (baseAudioEvent == null)
            {
                return;
            }

            if (!(baseAudioEvent is AudioEvent audioEvent))
            {
                // TODO
                return;
            }

            var entry = ResolveAudioEventEntry(audioEvent);

            if (entry == null)
            {
                logger.Warn($"Missing Audio File: {audioEvent.Name}");
                return;
            }

            var source = GetSound(entry, audioEvent.SubmixSlider, audioEvent.Control.HasFlag(AudioControlFlags.Loop));
            _sources.Add(source);

            source.Volume = (float) audioEvent.Volume;

            source.Play();
        }
    }
}

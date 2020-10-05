﻿using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data;
using OpenSage.Graphics.Cameras;
using OpenSage.Logic.Object;
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
        private readonly Audio3DEngine _3dengine;
        private readonly Dictionary<AudioVolumeSlider, Submixer> _mixers;

        private readonly Random _random;

        public AudioSystem(Game game) : base(game)
        {
            _engine = AudioEngine.CreateDefault();
            _3dengine = _engine.Create3DEngine();
            _sources = new List<AudioSource>();
            _cached = new Dictionary<string, AudioBuffer>();
            _mixers = new Dictionary<AudioVolumeSlider, Submixer>();
            _settings = game.AssetStore.AudioSettings.Current;

            CreateSubmixers();

            // TODO: Sync RNG seed from replay?
            _random = new Random();
        }

        public void Update(Camera camera)
        {
            UpdateListener(camera);
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
            return sound.AudioFile.Value?.Entry;
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

        private bool ValidateAudioEvent(BaseAudioEventInfo baseAudioEvent)
        {
            if (baseAudioEvent == null)
            {
                return false;
            }

            if (!(baseAudioEvent is AudioEvent))
            {
                // TODO
                return false;
            }

            return true;
        }

        private AudioSource PlayAudioEventBase(BaseAudioEventInfo baseAudioEvent)
        {
            if (!ValidateAudioEvent(baseAudioEvent))
            {
                return null;
            }

            var audioEvent = baseAudioEvent as AudioEvent;
            var entry = ResolveAudioEventEntry(audioEvent);

            if (entry == null)
            {
                logger.Warn($"Missing Audio File: {audioEvent.Name}");
                return null;
            }

            var source = GetSound(entry, audioEvent.SubmixSlider, audioEvent.Control.HasFlag(AudioControlFlags.Loop));
            source.Volume = (float) audioEvent.Volume;
            return source;
        }

        private void UpdateListener(Camera camera)
        {
            _3dengine.SetListenerPosition(camera.Position);
            var front = Vector3.Normalize(camera.Target - camera.Position);
            _3dengine.SetListenerOrientation(camera.Up, front);
        }

        public void PlayAudioEvent(GameObject emitter, BaseAudioEventInfo baseAudioEvent)
        {
            var source = PlayAudioEventBase(baseAudioEvent);
            if (source == null)
            {
                return;
            }

            // TODO: fix issues with some units
            //_3dengine.SetSourcePosition(source, emitter.Transform.Translation);
            source.Play();
        }

        public void PlayAudioEvent(BaseAudioEventInfo baseAudioEvent)
        {
            var source = PlayAudioEventBase(baseAudioEvent);
            if (source == null)
            {
                return;
            }

            source.Play();
        }
    }
}

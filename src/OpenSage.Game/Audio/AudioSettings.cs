﻿using System;
using System.IO;
using OpenSage.Data.Ini;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Audio
{
    public sealed class AudioSettings : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, AudioSettings value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<AudioSettings> FieldParseTable = new IniParseTable<AudioSettings>
        {
            { "AudioRoot", (parser, x) => x.AudioRoot = parser.ParseString() },
            { "SoundsFolder", (parser, x) => x.SoundsFolder = parser.ParseString() },
            { "MusicFolder", (parser, x) => x.MusicFolder = parser.ParseString() },
            { "StreamingFolder", (parser, x) => x.StreamingFolder = parser.ParseString() },
            { "AmbientStreamFolder", (parser, x) => x.AmbientStreamFolder = parser.ParseString() },
            { "SoundsExtension", (parser, x) => x.SoundsExtension = parser.ParseString() },
            { "UseDigital", (parser, x) => x.UseDigital = parser.ParseBoolean() },
            { "UseMidi", (parser, x) => x.UseMidi = parser.ParseBoolean() },
            { "OutputRate", (parser, x) => x.OutputRate = parser.ParseInteger() },
            { "OutputBits", (parser, x) => x.OutputBits = parser.ParseInteger() },
            { "OutputChannels", (parser, x) => x.OutputChannels = parser.ParseInteger() },
            { "MixaheadLatency", (parser, x) => x.MixaheadLatency = parser.ParseInteger() },
            { "MixaheadLatencyDuringMovies", (parser, x) => x.MixaheadLatencyDuringMovies = parser.ParseInteger() },
            { "3DBufferLengthMS", (parser, x) => x._3DBufferLengthMS = parser.ParseInteger() },
            { "3DBufferCallbackCallsPerBufferLength", (parser, x) => x._3DBufferCallbackCallsPerBufferLength = parser.ParseInteger() },
            { "ForceResetTimeSeconds", (parser, x) => x.ForceResetTime = parser.ParseTimeSeconds() },
            { "EmergencyResetTimeSeconds", (parser, x) => x.EmergencyResetTime = parser.ParseTimeSeconds() },
            { "MusicScriptLibraryName", (parser, x) => x.MusicScriptLibraryName = parser.ParseFileName() },
            { "AutomaticSubtitleDurationMS", (parser, x) => x.AutomaticSubtitleDuration = parser.ParseTimeMilliseconds() },
            { "AutomaticSubtitleWindowWidth", (parser, x) => x.AutomaticSubtitleWindowWidth = parser.ParseInteger() },
            { "AutomaticSubtitleLines", (parser, x) => x.AutomaticSubtitleLines = parser.ParseInteger() },
            { "AutomaticSubtitleWindowColor", (parser, x) => x.AutomaticSubtitleWindowColor = parser.ParseColorRgba().ToColorRgbaF() },
            { "AutomaticSubtitleTextColor", (parser, x) => x.AutomaticSubtitleTextColor = parser.ParseColorRgba().ToColorRgbaF() },
            { "PositionDeltaForReverbRecheck", (parser, x) => x.PositionDeltaForReverbRecheck = parser.ParseInteger() },
            { "SampleCount2D", (parser, x) => x.SampleCount2D = parser.ParseInteger() },
            { "SampleCount3D", (parser, x) => x.SampleCount3D = parser.ParseInteger() },
            { "StreamCount", (parser, x) => x.StreamCount = parser.ParseInteger() },
            { "GlobalMinRange", (parser, x) => x.GlobalMinRange = parser.ParseFloat() },
            { "GlobalMaxRange", (parser, x) => x.GlobalMaxRange = parser.ParseFloat() },
            { "TimeBetweenDrawableSounds", (parser, x) => x.TimeBetweenDrawableSounds = parser.ParseInteger() },
            { "TimeToFadeAudio", (parser, x) => x.TimeToFadeAudio = parser.ParseTimeMilliseconds() },
            { "AudioFootprintInBytes", (parser, x) => x.AudioFootprintInBytes = parser.ParseInteger() },
            { "MinSampleVolume", (parser, x) => x.MinSampleVolume = parser.ParseInteger() },
            { "AmbientStreamHysteresisVolume", (parser, x) => x.AmbientStreamHysteresisVolume = parser.ParseInteger() },

            { "Relative2DVolume", (parser, x) => x.Relative2DVolume = parser.ParsePercentage() },

            { "DefaultSoundVolume", (parser, x) => x.DefaultSoundVolume = parser.ParsePercentage() },
            { "DefaultAmbientVolume", (parser, x) => x.DefaultAmbientVolume = parser.ParsePercentage() },
            { "DefaultMovieVolume", (parser, x) => x.DefaultMovieVolume = parser.ParsePercentage() },
            { "DefaultVoiceVolume", (parser, x) => x.DefaultVoiceVolume = parser.ParsePercentage() },
            { "Default3DSoundVolume", (parser, x) => x.Default3DSoundVolume = parser.ParsePercentage() },
            { "DefaultSpeechVolume", (parser, x) => x.DefaultSpeechVolume = parser.ParsePercentage() },
            { "DefaultMusicVolume", (parser, x) => x.DefaultMusicVolume = parser.ParsePercentage() },
            { "Default2DSpeakerType", (parser, x) => x.Default2DSpeakerType = parser.ParseString() },
            { "Default3DSpeakerType", (parser, x) => x.Default3DSpeakerType = parser.ParseString() },

            { "Preferred3DHW1", (parser, x) => x.Preferred3DHW1 = parser.ParseString() },
            { "Preferred3DHW2", (parser, x) => x.Preferred3DHW2 = parser.ParseString() },
            { "Preferred3DSW", (parser, x) => x.Preferred3DSW = parser.ParseString() },

            { "MicrophoneDesiredHeightAboveTerrain", (parser, x) => x.MicrophoneDesiredHeightAboveTerrain = parser.ParseFloat() },
            { "MicrophoneMaxPercentageBetweenGroundAndCamera", (parser, x) => x.MicrophoneMaxPercentageBetweenGroundAndCamera = parser.ParsePercentage() },
            { "MicrophonePreferredFractionCameraToGround", (parser, x) => x.MicrophonePreferredFractionCameraToGround = parser.ParsePercentage() },
            { "MicrophoneMinDistanceToCamera", (parser, x) => x.MicrophoneMinDistanceToCamera = parser.ParseFloat() },
            { "MicrophoneMaxDistanceToCamera", (parser, x) => x.MicrophoneMaxDistanceToCamera = parser.ParseFloat() },
            { "MicrophonePullTowardsTerrainLookAtPointPercent", (parser, x) => x.MicrophonePullTowardsTerrainLookAtPointPercent = parser.ParsePercentage() },

            { "ZoomMinDistance", (parser, x) => x.ZoomMinDistance = parser.ParseFloat() },
            { "ZoomMaxDistance", (parser, x) => x.ZoomMaxDistance = parser.ParseFloat() },

            { "ZoomSoundVolumePercentageAmount", (parser, x) => x.ZoomSoundVolumePercentageAmount = parser.ParsePercentage() },

            { "LivingWorldMicrophonePreferredFractionCameraToGround", (parser, x) => x.LivingWorldMicrophonePreferredFractionCameraToGround = parser.ParsePercentage() },
            { "LivingWorldMicrophoneMaxDistanceToCamera", (parser, x) => x.LivingWorldMicrophoneMaxDistanceToCamera = parser.ParseFloat() },
            { "LivingWorldZoomMaxDistance", (parser, x) => x.LivingWorldZoomMaxDistance = parser.ParseFloat() },

            { "LoopBufferLengthMS", (parser, x) => x.LoopBufferLengthMS = parser.ParseInteger() },
            { "LoopBufferCallbackCallsPerBufferLength", (parser, x) => x.LoopBufferCallbackCallsPerBufferLength = parser.ParseInteger() },
            { "MillisecondsPriorToPlayingToReadSoundFile", (parser, x) => x.MillisecondsPriorToPlayingToReadSoundFile = parser.ParseInteger() },

            { "SuppressOcclusion", (parser, x) => x.SuppressOcclusion = parser.ParseBoolean() },
            { "MinOcclusion", (parser, x) => x.MinOcclusion = parser.ParsePercentage() },

            { "ZoomFadeDistanceForMaxEffect", (parser, x) => x.ZoomFadeDistanceForMaxEffect = parser.ParseInteger() },
            { "ZoomFadeZeroEffectEdgeLength", (parser, x) => x.ZoomFadeZeroEffectEdgeLength = parser.ParseInteger() },
            { "ZoomFadeFullEffectEdgeLength", (parser, x) => x.ZoomFadeFullEffectEdgeLength = parser.ParseInteger() },

            { "GlobalPaddedCellReverbMultiplier", (parser, x) => x.GlobalPaddedCellReverbMultiplier = parser.ParsePercentage() },
            { "GlobalRoomReverbMultiplier", (parser, x) => x.GlobalRoomReverbMultiplier = parser.ParsePercentage() },
            { "GlobalBathroomReverbMultiplier", (parser, x) => x.GlobalBathroomReverbMultiplier = parser.ParsePercentage() },
            { "GlobalLivingRoomReverbMultiplier", (parser, x) => x.GlobalLivingRoomReverbMultiplier = parser.ParsePercentage() },
            { "GlobalStoneRoomReverbMultiplier", (parser, x) => x.GlobalStoneRoomReverbMultiplier = parser.ParsePercentage() },
            { "GlobalAuditoriumReverbMultiplier", (parser, x) => x.GlobalAuditoriumReverbMultiplier = parser.ParsePercentage() },
            { "GlobalConcertHallReverbMultiplier", (parser, x) => x.GlobalConcertHallReverbMultiplier = parser.ParsePercentage() },
            { "GlobalCaveReverbMultiplier", (parser, x) => x.GlobalCaveReverbMultiplier = parser.ParsePercentage() },
            { "GlobalArenaReverbMultiplier", (parser, x) => x.GlobalArenaReverbMultiplier = parser.ParsePercentage() },
            { "GlobalHangarReverbMultiplier", (parser, x) => x.GlobalHangarReverbMultiplier = parser.ParsePercentage() },
            { "GlobalCarpetedHallwayReverbMultiplier", (parser, x) => x.GlobalCarpetedHallwayReverbMultiplier = parser.ParsePercentage() },
            { "GlobalHallwayReverbMultiplier", (parser, x) => x.GlobalHallwayReverbMultiplier = parser.ParsePercentage() },
            { "GlobalStoneCorridorReverbMultiplier", (parser, x) => x.GlobalStoneCorridorReverbMultiplier = parser.ParsePercentage() },
            { "GlobalAlleyReverbMultiplier", (parser, x) => x.GlobalAlleyReverbMultiplier = parser.ParsePercentage() },
            { "GlobalForestReverbMultiplier", (parser, x) => x.GlobalForestReverbMultiplier = parser.ParsePercentage() },
            { "GlobalCityReverbMultiplier", (parser, x) => x.GlobalCityReverbMultiplier = parser.ParsePercentage() },
            { "GlobalMountainsReverbMultiplier", (parser, x) => x.GlobalMountainsReverbMultiplier = parser.ParsePercentage() },
            { "GlobalQuarryReverbMultiplier", (parser, x) => x.GlobalQuarryReverbMultiplier = parser.ParsePercentage() },
            { "GlobalPlainReverbMultiplier", (parser, x) => x.GlobalPlainReverbMultiplier = parser.ParsePercentage() },
            { "GlobalParkingLotReverbMultiplier", (parser, x) => x.GlobalParkingLotReverbMultiplier = parser.ParsePercentage() },
            { "GlobalSewerPipeReverbMultiplier", (parser, x) => x.GlobalSewerPipeReverbMultiplier = parser.ParsePercentage() },
            { "GlobalUnderwaterReverbMultiplier", (parser, x) => x.GlobalUnderwaterReverbMultiplier = parser.ParsePercentage() },
            { "GlobalDruggedReverbMultiplier", (parser, x) => x.GlobalDruggedReverbMultiplier = parser.ParsePercentage() },
            { "GlobalDizzyReverbMultiplier", (parser, x) => x.GlobalDizzyReverbMultiplier = parser.ParsePercentage() },
            { "GlobalPsychoticReverbMultiplier", (parser, x) => x.GlobalPsychoticReverbMultiplier = parser.ParsePercentage() },

            { "VoiceMoveToCampMaxCampnessAtStartPoint", (parser, x) => x.VoiceMoveToCampMaxCampnessAtStartPoint = parser.ParseInteger() },
            { "VoiceMoveToCampMinCampnessAtEndPoint", (parser, x) => x.VoiceMoveToCampMinCampnessAtEndPoint = parser.ParseInteger() },
            { "MinDelayBetweenEnterStateVoiceMS", (parser, x) => x.MinDelayBetweenEnterStateVoice = parser.ParseTimeMilliseconds() },
        };

        internal static AudioSettings ParseAsset(BinaryReader reader, Asset asset)
        {
            var result = new AudioSettings();
            result.SetNameAndInstanceId(asset);

            var streamBufferSizePerChannel = reader.ReadUInt32();
            var maxRequestsPerStream = reader.ReadUInt32();
            result.ForceResetTime = reader.ReadTime();
            result.EmergencyResetTime = reader.ReadTime();
            result.MinOcclusion = reader.ReadPercentage();
            var delayPriorToPlayingToReadSoundFile = reader.ReadSingle();
            var readAheadTime = reader.ReadSingle();
            var queueAheadTime = reader.ReadSingle();
            var longTime = reader.ReadSingle();
            result.AudioFootprintInBytes = reader.ReadInt32();

            result.DefaultSoundVolume = reader.ReadPercentage();
            result.DefaultVoiceVolume = reader.ReadPercentage();
            result.DefaultMusicVolume = reader.ReadPercentage();
            result.DefaultMovieVolume = reader.ReadPercentage();
            result.DefaultAmbientVolume = reader.ReadPercentage();
            result.AutomaticSubtitleDuration = reader.ReadTime();
            result.AutomaticSubtitleLines = reader.ReadInt32();
            var panRadiusScaleValue = reader.ReadSingle();
            var panSize = reader.ReadSingle();
            result.MinSampleVolume = reader.ReadSingle();
            result.PositionDeltaForReverbRecheck = reader.ReadSingle();
            result.TimeToFadeAudio = reader.ReadTime();
            result.AmbientStreamHysteresisVolume = reader.ReadInt32();
            var volumeMultiplierFor2DSounds = reader.ReadSingle();

            result.VoiceMoveToCampMaxCampnessAtStartPoint = reader.ReadInt32();
            result.VoiceMoveToCampMinCampnessAtEndPoint = reader.ReadInt32();
            result.MinDelayBetweenEnterStateVoice = reader.ReadTime();
            var timeSinceLastAttackForVoiceRetreatToCastle = reader.ReadSingle();
            var distanceToLookForEnemiesForVoiceRetreatToCastle = reader.ReadSingle();
            var postPostGameMusicWait = reader.ReadSingle();

            result.AutomaticSubtitleWindowColor = reader.ReadColorRgbaF();
            result.AutomaticSubtitleTextColor = reader.ReadColorRgbaF();

            result.VolumeSliderMultipliersForMovie = reader.ReadArrayAtOffset(() => VolumeSliderMultiplier.ParseAsset(reader));

            result.TacticalMicSettings = MicrophoneSettings.ParseAsset(reader);
            result.LivingWorldMicSettings = MicrophoneSettings.ParseAsset(reader);
            result.ShellMicSettings = MicrophoneSettings.ParseAsset(reader);

            result.VolumeCompressionSettings = VolumeCompressionSettings.ParseAsset(reader);

            result.SuppressOcclusion = reader.ReadBooleanChecked();
            var playEnemySightedEventsOnlyFromNearbyUnits = reader.ReadBooleanChecked();

            return result;
        }

        public string AudioRoot { get; private set; }
        public string SoundsFolder { get; private set; }
        public string MusicFolder { get; private set; }
        public string StreamingFolder { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string AmbientStreamFolder { get; private set; }

        public string SoundsExtension { get; private set; }
        public bool UseDigital { get; private set; }
        public bool UseMidi { get; private set; }
        public int OutputRate { get; private set; }
        public int OutputBits { get; private set; }
        public int OutputChannels { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MixaheadLatency { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MixaheadLatencyDuringMovies { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int _3DBufferLengthMS { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int _3DBufferCallbackCallsPerBufferLength { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public TimeSpan ForceResetTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public TimeSpan EmergencyResetTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string MusicScriptLibraryName { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public TimeSpan AutomaticSubtitleDuration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int AutomaticSubtitleWindowWidth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int AutomaticSubtitleLines { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgbaF AutomaticSubtitleWindowColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ColorRgbaF AutomaticSubtitleTextColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float PositionDeltaForReverbRecheck { get; private set; }

        public int SampleCount2D { get; private set; }
        public int SampleCount3D { get; private set; }
        public int StreamCount { get; private set; }
        public float GlobalMinRange { get; private set; }
        public float GlobalMaxRange { get; private set; }
        public int TimeBetweenDrawableSounds { get; private set; }
        public TimeSpan TimeToFadeAudio { get; private set; }
        public int AudioFootprintInBytes { get; private set; }
        public float MinSampleVolume { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int AmbientStreamHysteresisVolume { get; private set; }

        public Percentage Relative2DVolume { get; private set; }

        public Percentage DefaultSoundVolume { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage DefaultAmbientVolume { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage DefaultMovieVolume { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage DefaultVoiceVolume { get; private set; }

        public Percentage Default3DSoundVolume { get; private set; }
        public Percentage DefaultSpeechVolume { get; private set; }
        public Percentage DefaultMusicVolume { get; private set; }
        public string Default2DSpeakerType { get; private set; }
        public string Default3DSpeakerType { get; private set; }

        public string Preferred3DHW1 { get; private set; }
        public string Preferred3DHW2 { get; private set; }
        public string Preferred3DSW { get; private set; }

        public float MicrophoneDesiredHeightAboveTerrain { get; private set; }

        public Percentage MicrophoneMaxPercentageBetweenGroundAndCamera { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage MicrophonePreferredFractionCameraToGround { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MicrophoneMinDistanceToCamera { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MicrophoneMaxDistanceToCamera { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage MicrophonePullTowardsTerrainLookAtPointPercent { get; private set; }

        public float ZoomMinDistance { get; private set; }
        public float ZoomMaxDistance { get; private set; }

        public Percentage ZoomSoundVolumePercentageAmount { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Percentage LivingWorldMicrophonePreferredFractionCameraToGround { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float LivingWorldMicrophoneMaxDistanceToCamera { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float LivingWorldZoomMaxDistance { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int LoopBufferLengthMS { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int LoopBufferCallbackCallsPerBufferLength { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int MillisecondsPriorToPlayingToReadSoundFile { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool SuppressOcclusion { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int ZoomFadeDistanceForMaxEffect { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int ZoomFadeZeroEffectEdgeLength { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int ZoomFadeFullEffectEdgeLength { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage MinOcclusion { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalPaddedCellReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalRoomReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalBathroomReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalLivingRoomReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalStoneRoomReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalAuditoriumReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalConcertHallReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalCaveReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalArenaReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalHangarReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalCarpetedHallwayReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalHallwayReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalStoneCorridorReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalAlleyReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalForestReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalCityReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalMountainsReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalQuarryReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalPlainReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalParkingLotReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalSewerPipeReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalUnderwaterReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalDruggedReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalDizzyReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Percentage GlobalPsychoticReverbMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int VoiceMoveToCampMaxCampnessAtStartPoint { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int VoiceMoveToCampMinCampnessAtEndPoint { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public TimeSpan MinDelayBetweenEnterStateVoice { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public VolumeSliderMultiplier[] VolumeSliderMultipliersForMovie { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public MicrophoneSettings TacticalMicSettings { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public MicrophoneSettings LivingWorldMicSettings { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public MicrophoneSettings ShellMicSettings { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public VolumeCompressionSettings VolumeCompressionSettings { get; private set; }
    }

    public sealed class MicrophoneSettings
    {
        internal static MicrophoneSettings ParseAsset(BinaryReader reader)
        {
            return new MicrophoneSettings
            {
                MicrophonePreferredFractionCameraToGround = reader.ReadSingle(),
                VolumeMicrophonePullTowardsTerrainLookAtPoint = reader.ReadSingle(),
                PanningMicrophonePullTowardsTerrainLookAtPoint = reader.ReadSingle(),
                MicrophoneMinDistanceToCamera = reader.ReadSingle(),
                MicrophoneMaxDistanceToCamera = reader.ReadSingle(),
                ZoomMinDistance = reader.ReadSingle(),
                ZoomMaxDistance = reader.ReadSingle(),
                ZoomSoundVolumePercentageAmount = reader.ReadSingle(),
                ZoomFadeDistanceForMaxEffect = reader.ReadSingle(),
                ZoomFadeZeroEffectEdgeLength = reader.ReadSingle(),
                ZoomFadeFullEffectEdgeLength = reader.ReadSingle()
            };
        }

        public float MicrophonePreferredFractionCameraToGround { get; private set; }
        public float VolumeMicrophonePullTowardsTerrainLookAtPoint { get; private set; }
        public float PanningMicrophonePullTowardsTerrainLookAtPoint { get; private set; }
        public float MicrophoneMinDistanceToCamera { get; private set; }
        public float MicrophoneMaxDistanceToCamera { get; private set; }
        public float ZoomMinDistance { get; private set; }
        public float ZoomMaxDistance { get; private set; }
        public float ZoomSoundVolumePercentageAmount { get; private set; }
        public float ZoomFadeDistanceForMaxEffect { get; private set; }
        public float ZoomFadeZeroEffectEdgeLength { get; private set; }
        public float ZoomFadeFullEffectEdgeLength { get; private set; }
    }

    public sealed class VolumeCompressionSettings
    {
        internal static VolumeCompressionSettings ParseAsset(BinaryReader reader)
        {
            return new VolumeCompressionSettings
            {
                Threshold = reader.ReadSingle(),
                Ratio = reader.ReadSingle(),
                AttackTime = reader.ReadTime(),
                ReleaseTime = reader.ReadTime(),
                AppliesEquallyToAllChannels = reader.ReadBooleanChecked()
            };
        }

        public float Threshold { get; private set; }
        public float Ratio { get; private set; }
        public TimeSpan AttackTime { get; private set; }
        public TimeSpan ReleaseTime { get; private set; }
        public bool AppliesEquallyToAllChannels { get; private set; }
    }
}

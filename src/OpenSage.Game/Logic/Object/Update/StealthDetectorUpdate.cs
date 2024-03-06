using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.Logic.Object
{
    public sealed class StealthDetectorUpdate : UpdateModule
    {
        private readonly StealthDetectorUpdateModuleData _moduleData;
        public bool Active;

        protected override LogicFrameSpan FramesBetweenUpdates => _moduleData.DetectionRate;

        public StealthDetectorUpdate(StealthDetectorUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
            Active = !_moduleData.InitiallyDisabled;
        }

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            // todo: detect stealth
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistBoolean(ref Active);
        }
    }

    /// <summary>
    /// Display MESSAGE:StealthDiscovered when triggered.
    /// </summary>
    public sealed class StealthDetectorUpdateModuleData : UpdateModuleData
    {
        internal static StealthDetectorUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StealthDetectorUpdateModuleData> FieldParseTable = new IniParseTable<StealthDetectorUpdateModuleData>
        {
            { "DetectionRate", (parser, x) => x.DetectionRate = parser.ParseTimeMillisecondsToLogicFrames() },
            { "InitiallyDisabled", (parser, x) => x.InitiallyDisabled = parser.ParseBoolean() },
            { "DetectionRange", (parser, x) => x.DetectionRange = parser.ParseInteger() },
            { "CanDetectWhileGarrisoned", (parser, x) => x.CanDetectWhileGarrisoned = parser.ParseBoolean() },
            { "CanDetectWhileContained", (parser, x) => x.CanDetectWhileContained = parser.ParseBoolean() },
            { "ExtraRequiredKindOf", (parser, x) => x.ExtraRequiredKindOf = parser.ParseEnum<ObjectKinds>() },
            { "PingSound", (parser, x) => x.PingSound = parser.ParseAudioEventReference() },
            { "LoudPingSound", (parser, x) => x.LoudPingSound = parser.ParseAudioEventReference() },
            { "IRParticleSysName", (parser, x) => x.IRParticleSysName = parser.ParseFXParticleSystemTemplateReference() },
            { "IRBrightParticleSysName", (parser, x) => x.IRBrightParticleSysName = parser.ParseFXParticleSystemTemplateReference() },
            { "IRGridParticleSysName", (parser, x) => x.IRGridParticleSysName = parser.ParseFXParticleSystemTemplateReference() },
            { "IRBeaconParticleSysName", (parser, x) => x.IRBeaconParticleSysName = parser.ParseFXParticleSystemTemplateReference() },
            { "IRParticleSysBone", (parser, x) => x.IRParticleSysBone = parser.ParseBoneName() },
            { "CancelOneRingEffect", (parser, x) => x.CancelOneRingEffect = parser.ParseBoolean() },
            { "RequiredUpgrade", (parser, x) => x.RequiredUpgrade = parser.ParseAssetReference() },
        };

        /// <summary>
        /// How often, in milliseconds, to scan for stealthed objects in sight range.
        /// </summary>
        public LogicFrameSpan DetectionRate { get; private set; }

        public bool InitiallyDisabled { get; private set; }

        public int DetectionRange { get; private set; }

        public bool CanDetectWhileGarrisoned { get; private set; }

        public bool CanDetectWhileContained { get; private set; }

        public ObjectKinds ExtraRequiredKindOf { get; private set; }

        public LazyAssetReference<BaseAudioEventInfo> PingSound { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> LoudPingSound { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> IRParticleSysName { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> IRBrightParticleSysName { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> IRGridParticleSysName { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> IRBeaconParticleSysName { get; private set; }
        public string IRParticleSysBone { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool CancelOneRingEffect { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string RequiredUpgrade { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new StealthDetectorUpdate(this);
        }
    }
}

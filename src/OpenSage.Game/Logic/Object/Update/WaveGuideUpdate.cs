using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class WaveGuideUpdate : UpdateModule
    {
        // TODO

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.SkipUnknownBytes(2330);
        }
    }

    /// <summary>
    /// Hardcoded to use the following particle system definitions: WaveSpray03, WaveSpray02, 
    /// WaveSpray01, WaveSplashRight01, WaveSplashLeft01, WaveHit01, WaveSplash01 and also uses the 
    /// WaterWaveBridge object definition as a template when it collides with a bridge.
    /// Requires a WAVEGUIDE KindOf.
    /// </summary>
    public sealed class WaveGuideUpdateModuleData : UpdateModuleData
    {
        internal static WaveGuideUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WaveGuideUpdateModuleData> FieldParseTable = new IniParseTable<WaveGuideUpdateModuleData>
        {
            { "WaveDelay", (parser, x) => x.WaveDelay = parser.ParseInteger() },
            { "YSize", (parser, x) => x.YSize = parser.ParseFloat() },
            { "LinearWaveSpacing", (parser, x) => x.LinearWaveSpacing = parser.ParseFloat() },
            { "WaveBendMagnitude", (parser, x) => x.WaveBendMagnitude = parser.ParseFloat() },
            { "WaterVelocity", (parser, x) => x.WaterVelocity = parser.ParseFloat() },
            { "PreferredHeight", (parser, x) => x.PreferredHeight = parser.ParseFloat() },
            { "ShorelineEffectDistance", (parser, x) => x.ShorelineEffectDistance = parser.ParseFloat() },
            { "DamageRadius", (parser, x) => x.DamageRadius = parser.ParseFloat() },
            { "DamageAmount", (parser, x) => x.DamageAmount = parser.ParseInteger() },
            { "ToppleForce", (parser, x) => x.ToppleForce = parser.ParseFloat() },
            { "RandomSplashSound", (parser, x) => x.RandomSplashSound = parser.ParseAssetReference() },
            { "RandomSplashSoundFrequency", (parser, x) => x.RandomSplashSoundFrequency = parser.ParseInteger() },
            { "BridgeParticle", (parser, x) => x.BridgeParticle = parser.ParseAssetReference() },
            { "BridgeParticleAngleFudge", (parser, x) => x.BridgeParticleAngleFudge = parser.ParseFloat() },
            { "LoopingSound", (parser, x) => x.LoopingSound = parser.ParseAssetReference() },
        };

        public int WaveDelay { get; private set; }
        public float YSize { get; private set; }
        public float LinearWaveSpacing { get; private set; }
        public float WaveBendMagnitude { get; private set; }
        public float WaterVelocity { get; private set; }
        public float PreferredHeight { get; private set; }
        public float ShorelineEffectDistance { get; private set; }
        public float DamageRadius { get; private set; }
        public int DamageAmount { get; private set; }
        public float ToppleForce { get; private set; }
        public string RandomSplashSound { get; private set; }
        public int RandomSplashSoundFrequency { get; private set; }
        public string BridgeParticle { get; private set; }
        public float BridgeParticleAngleFudge { get; private set; }
        public string LoopingSound { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new WaveGuideUpdate();
        }
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class FlammableUpdate : UpdateModule
    {
        private readonly FlammableUpdateModuleData _moduleData;

        private FlammableState _state;
        private uint _aflameEndFrame;
        private uint _nextDamageFrame;
        private float _remainingDamageBeforeCatchingFire;
        private uint _startedTakingFlameDamageFrame;

        internal FlammableUpdate(FlammableUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
            _remainingDamageBeforeCatchingFire = moduleData.FlameDamageLimit;
        }

        // TODO

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _state = reader.ReadEnum<FlammableState>();

            _aflameEndFrame = reader.ReadFrame();

            reader.SkipUnknownBytes(4);

            _nextDamageFrame = reader.ReadFrame();

            _remainingDamageBeforeCatchingFire = reader.ReadSingle();

            _startedTakingFlameDamageFrame = reader.ReadFrame();
        }

        private enum FlammableState
        {
            Inactive = 0,
            Aflame = 1,
        }
    }

    /// <summary>
    /// Allows the use of the AFLAME, SMOLDERING, and BURNED condition states.
    /// </summary>
    public sealed class FlammableUpdateModuleData : UpdateModuleData
    {
        internal static FlammableUpdateModuleData Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<FlammableUpdateModuleData> FieldParseTable = new IniParseTable<FlammableUpdateModuleData>
        {
            { "FlameDamageLimit", (parser, x) => x.FlameDamageLimit = parser.ParseFloat() },
            { "FlameDamageExpiration", (parser, x) => x.FlameDamageExpiration = parser.ParseInteger() },
            { "AflameDuration", (parser, x) => x.AflameDuration = parser.ParseInteger() },
            { "AflameDamageAmount", (parser, x) => x.AflameDamageAmount = parser.ParseInteger() },
            { "AflameDamageDelay", (parser, x) => x.AflameDamageDelay = parser.ParseInteger() },
            { "BurnedDelay", (parser, x) => x.BurnedDelay = parser.ParseInteger() },
            { "BurningSoundName", (parser, x) => x.BurningSoundName = parser.ParseAssetReference() },
            { "BurnContained", (parser, x) => x.BurnContained = parser.ParseBoolean() },
            { "FireFXList", (parser, x) => x.FireFXList = parser.ParseAttributeIdentifier("FX") },
            { "SwapModelWhenAflame", (parser, x) => x.SwapModelWhenAflame = parser.ParseBoolean() },
            { "SwapModelWhenQuenched", (parser, x) => x.SwapModelWhenQuenched = parser.ParseBoolean() },
            { "RunToWater", (parser, x) => x.RunToWater = parser.ParseBoolean() },
            { "RunToWaterDepth", (parser, x) => x.RunToWaterDepth = parser.ParseInteger() },
            { "RunToWaterSearchRadius", (parser, x) => x.RunToWaterSearchRadius  = parser.ParseInteger() },
            { "RunToWaterSearchIncrement", (parser, x) => x.RunToWaterSearchIncrement = parser.ParseInteger() },
            { "PanicLocomotorWhileAflame", (parser, x) => x.PanicLocomotorWhileAflame = parser.ParseBoolean() },
            { "CustomAnimAndDuration", (parser, x) => x.CustomAnimAndDuration = AnimAndDuration.Parse(parser) },
            { "SetBurnedStatus", (parser, x) => x.SetBurnedStatus = parser.ParseBoolean() },
            { "DamageType", (parser, x) => x.DamageType = parser.ParseEnum<DamageType>() }
        };

        /// <summary>
        /// How much flame damage to receive before catching fire.
        /// </summary>
        public float FlameDamageLimit { get; private set; } = 20.0f;

        /// <summary>
        /// Time within which <see cref="FlameDamageLimit"/> must be received in order to catch fire.
        /// </summary>
        public int FlameDamageExpiration { get; private set; }

        /// <summary>
        /// How long to burn for after catching fire.
        /// </summary>
        public int AflameDuration { get; private set; }

        /// <summary>
        /// Amount of damage inflicted.
        /// </summary>
        public int AflameDamageAmount { get; private set; }

        /// <summary>
        /// Delay between each time that <see cref="AflameDamageAmount"/> is inflicted.
        /// </summary>
        public int AflameDamageDelay { get; private set; }

        public int BurnedDelay { get; private set; }

        public string BurningSoundName { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool BurnContained { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string FireFXList { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool SwapModelWhenAflame { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool SwapModelWhenQuenched { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RunToWater { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int RunToWaterDepth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int RunToWaterSearchRadius { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int RunToWaterSearchIncrement { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool PanicLocomotorWhileAflame { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public AnimAndDuration CustomAnimAndDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool SetBurnedStatus { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public DamageType DamageType { get; internal set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new FlammableUpdate(this);
        }
    }
}

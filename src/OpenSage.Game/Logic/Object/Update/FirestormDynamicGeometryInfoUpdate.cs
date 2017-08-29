using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FirestormDynamicGeometryInfoUpdateModuleData : UpdateModuleData
    {
        internal static FirestormDynamicGeometryInfoUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FirestormDynamicGeometryInfoUpdateModuleData> FieldParseTable = new IniParseTable<FirestormDynamicGeometryInfoUpdateModuleData>
        {
            { "InitialDelay", (parser, x) => x.InitialDelay = parser.ParseInteger() },
            { "InitialHeight", (parser, x) => x.InitialHeight = parser.ParseFloat() },
            { "InitialMajorRadius", (parser, x) => x.InitialMajorRadius = parser.ParseFloat() },

            { "FinalHeight", (parser, x) => x.FinalHeight = parser.ParseFloat() },
            { "FinalMajorRadius", (parser, x) => x.FinalMajorRadius = parser.ParseFloat() },

            { "TransitionTime", (parser, x) => x.TransitionTime = parser.ParseInteger() },
            { "ReverseAtTransitionTime", (parser, x) => x.ReverseAtTransitionTime = parser.ParseBoolean() },

            { "ScorchSize", (parser, x) => x.ScorchSize = parser.ParseFloat() },
            { "ParticleOffsetZ", (parser, x) => x.ParticleOffsetZ = parser.ParseFloat() },
            { "ParticleSystem1", (parser, x) => x.ParticleSystem1 = parser.ParseAssetReference() },
            { "ParticleSystem2", (parser, x) => x.ParticleSystem2 = parser.ParseAssetReference() },
            { "FXList", (parser, x) => x.FXList = parser.ParseAssetReference() },

            { "DelayBetweenDamageFrames", (parser, x) => x.DelayBetweenDamageFrames = parser.ParseInteger() },
            { "DamageAmount", (parser, x) => x.DamageAmount = parser.ParseFloat() },
        };

        public int InitialDelay { get; private set; }
        public float InitialHeight { get; private set; }
        public float InitialMajorRadius { get; private set; }

        public float FinalHeight { get; private set; }
        public float FinalMajorRadius { get; private set; }
        
        public int TransitionTime { get; private set; }
        public bool ReverseAtTransitionTime { get; private set; }

        public float ScorchSize { get; private set; }
        public float ParticleOffsetZ { get; private set; }
        public string ParticleSystem1 { get; private set; }
        public string ParticleSystem2 { get; private set; }
        public string FXList { get; private set; }

        public int DelayBetweenDamageFrames { get; private set; }
        public float DamageAmount { get; private set; }
    }
}

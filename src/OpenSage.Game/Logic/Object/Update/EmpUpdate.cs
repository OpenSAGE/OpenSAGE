using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class EmpUpdateModuleData : UpdateModuleData
    {
        internal static EmpUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EmpUpdateModuleData> FieldParseTable = new IniParseTable<EmpUpdateModuleData>
        {
            { "DisabledDuration", (parser, x) => x.DisabledDuration = parser.ParseInteger() },
            { "Lifetime", (parser, x) => x.Lifetime = parser.ParseInteger() },
            { "StartFadeTime", (parser, x) => x.StartFadeTime = parser.ParseInteger() },
            { "StartScale", (parser, x) => x.StartScale = parser.ParseFloat() },
            { "TargetScaleMin", (parser, x) => x.TargetScaleMin = parser.ParseFloat() },
            { "TargetScaleMax", (parser, x) => x.TargetScaleMax = parser.ParseFloat() },
            { "StartColor", (parser, x) => x.StartColor = IniColorRgb.Parse(parser) },
            { "EndColor", (parser, x) => x.EndColor = IniColorRgb.Parse(parser) },
            { "DisableFXParticleSystem", (parser, x) => x.DisableFXParticleSystem = parser.ParseAssetReference() },
            { "DoesNotAffect", (parser, x) => x.DoesNotAffect = parser.ParseEnum<WeaponAffectsTypes>() },
            { "DoesNotAffectMyOwnBuildings", (parser, x) => x.DoesNotAffectMyOwnBuildings = parser.ParseBoolean() },
            { "EffectRadius", (parser, x) => x.EffectRadius = parser.ParseInteger() },
        };

        public int DisabledDuration { get; private set; }
        public int Lifetime { get; private set; }
        public int StartFadeTime { get; private set; }
        public float StartScale { get; private set; }
        public float TargetScaleMin { get; private set; }
        public float TargetScaleMax { get; private set; }
        public IniColorRgb StartColor { get; private set; }
        public IniColorRgb EndColor { get; private set; }
        public string DisableFXParticleSystem { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public WeaponAffectsTypes DoesNotAffect { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool DoesNotAffectMyOwnBuildings { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int EffectRadius { get; private set; }
    }
}

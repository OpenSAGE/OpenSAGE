using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    
    public sealed class SpecialDisguiseUpdateModuleData : UpdateModuleData
    {
        internal static SpecialDisguiseUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialDisguiseUpdateModuleData> FieldParseTable = new IniParseTable<SpecialDisguiseUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseIdentifier() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "OpacityTarget", (parser, x) => x.OpacityTarget = parser.ParseFloat() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "DisguiseAsTemplate", (parser, x) => x.DisguiseAsTemplate = parser.ParseIdentifier() },
            { "DisguisedAsTemplate_EnemyPerspective", (parser, x) => x.DisguisedAsTemplate_EnemyPerspective = parser.ParseIdentifier() },
            { "DisguiseFX", (parser, x) => x.DisguiseFX = parser.ParseAssetReference() },
            { "ForceMountedWhenDisguising", (parser, x) => x.ForceMountedWhenDisguising = parser.ParseBoolean() }
        };
        public string SpecialPowerTemplate { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int PackTime { get; private set; }
        public float OpacityTarget { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public string DisguiseAsTemplate { get; private set; }
        public string DisguisedAsTemplate_EnemyPerspective { get; private set; }
        public string DisguiseFX { get; private set; }
        public bool ForceMountedWhenDisguising { get; private set; }
    }
}

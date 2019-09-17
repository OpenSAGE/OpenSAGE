using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ToppleUpdateModuleData : UpdateModuleData
    {
        internal static ToppleUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ToppleUpdateModuleData> FieldParseTable = new IniParseTable<ToppleUpdateModuleData>
        {
            { "ToppleFX", (parser, x) => x.ToppleFX = parser.ParseAssetReference() },
            { "BounceFX", (parser, x) => x.BounceFX = parser.ParseAssetReference() },
            { "KillWhenStartToppling", (parser, x) => x.KillWhenStartToppling = parser.ParseBoolean() },
            { "ToppleLeftOrRightOnly", (parser, x) => x.ToppleLeftOrRightOnly = parser.ParseBoolean() },
            { "ReorientToppledRubble", (parser, x) => x.ReorientToppledRubble = parser.ParseBoolean() },
            { "BounceVelocityPercent", (parser, x) => x.BounceVelocityPercent = parser.ParsePercentage() },
            { "InitialAccelPercent", (parser, x) => x.InitialAccelPercent = parser.ParsePercentage() },
            { "StumpName", (parser, x) => x.StumpName = parser.ParseAssetReference() },
        };

        public string ToppleFX { get; private set; }
        public string BounceFX { get; private set; }
        public bool KillWhenStartToppling { get; private set; }
        public bool ToppleLeftOrRightOnly { get; private set; }
        public bool ReorientToppledRubble { get; private set; }
        public Percentage BounceVelocityPercent { get; private set; } = new Percentage(0.3f);
        public Percentage InitialAccelPercent { get; private set; } = new Percentage(0.01f);
        public string StumpName { get; private set; }
    }
}

using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class MoneyCrateCollideBehavior : ObjectBehavior
    {
        internal static MoneyCrateCollideBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MoneyCrateCollideBehavior> FieldParseTable = new IniParseTable<MoneyCrateCollideBehavior>
        {
            { "BuildingPickup", (parser, x) => x.BuildingPickup = parser.ParseBoolean() },
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "MoneyProvided", (parser, x) => x.MoneyProvided = parser.ParseInteger() },
            { "ExecuteAnimation", (parser, x) => x.ExecuteAnimation = parser.ParseAssetReference() },
            { "ExecuteAnimationTime", (parser, x) => x.ExecuteAnimationTime = parser.ParseFloat() },
            { "ExecuteAnimationZRise", (parser, x) => x.ExecuteAnimationZRise = parser.ParseFloat() },
            { "ExecuteAnimationFades", (parser, x) => x.ExecuteAnimationFades = parser.ParseBoolean() }
        };

        public bool BuildingPickup { get; private set; }
        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int MoneyProvided { get; private set; }
        public string ExecuteAnimation { get; private set; }
        public float ExecuteAnimationTime { get; private set; }
        public float ExecuteAnimationZRise { get; private set; }
        public bool ExecuteAnimationFades { get; private set; }
    }
}

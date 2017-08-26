using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Switches to a model condition state via upgrades.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class ModelConditionUpgrade : ObjectBehavior
    {
        internal static ModelConditionUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ModelConditionUpgrade> FieldParseTable = new IniParseTable<ModelConditionUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "ConditionFlag", (parser, x) => x.ConditionFlag = parser.ParseEnum<ModelConditionFlag>() },
        };

        public string TriggeredBy { get; private set; }
        public ModelConditionFlag ConditionFlag { get; private set; }
    }
}

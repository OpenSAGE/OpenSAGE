using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class ModelConditionAudioLoopClientBehaviorData : ClientBehaviorModuleData
    {
        internal static ModelConditionAudioLoopClientBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<ModelConditionAudioLoopClientBehaviorData> FieldParseTable = new IniParseTable<ModelConditionAudioLoopClientBehaviorData>
        {
            { "ModelCondition", (parser, x) => x.ModelCondition = ModelCondition.Parse(parser) }
        };

        public ModelCondition ModelCondition { get; private set; }
    }

    public sealed class ModelCondition
    {
        internal static ModelCondition Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<ModelCondition> FieldParseTable = new IniParseTable<ModelCondition>
        {
            { "REQUIRED", (parser, x) => x.Required = parser.ParseEnum<ModelConditionFlag>() },
            { "Sound", (parser, x) => x.Sound = parser.ParseAssetReference() },
            { "EXCLUDED", (parser, x) => x.Excluded = parser.ParseEnum<ModelConditionFlag>() }
        };

        public ModelConditionFlag Required { get; private set; }
        public ModelConditionFlag Excluded { get; private set; }
        public string Sound { get; private set; }
    }
}

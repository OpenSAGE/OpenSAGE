using OpenSage.Data.Ini;
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
        internal static ModelCondition Parse(IniParser parser)
        {
            //TODO: proper parsing, in line enum bit array not working properly
            var v = parser.ParseAssetReferenceArray();
            return new ModelCondition();

            //var v = parser.ParseAttributeList(FieldParseTable);
            //return v;
        }

        internal static readonly IniParseTable<ModelCondition> FieldParseTable = new IniParseTable<ModelCondition>
        {
            { "REQUIRED", (parser, x) => x.Required = parser.ParseEnum<ModelConditionFlag>() },
            { "Required", (parser, x) => x.Required = parser.ParseEnum<ModelConditionFlag>() },

            { "Sound", (parser, x) => x.Sound = parser.ParseAssetReference() },

            { "EXCLUDED", (parser, x) => x.Excluded = parser.ParseEnum<ModelConditionFlag>() },
            { "Excluded", (parser, x) => x.Excluded = parser.ParseEnum<ModelConditionFlag>() }
        };

        public ModelConditionFlag Required { get; private set; }
        public ModelConditionFlag Excluded { get; private set; }
        public string Sound { get; private set; }
    }
}

using System.Collections.Generic;
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

    public struct ModelCondition
    {
        internal static ModelCondition Parse(IniParser parser)
        {
            return new ModelCondition()
            {
                Required = parser.ParseAttributeEnum<ModelConditionFlag>("REQUIRED"),
                Sound = parser.ParseAttributeIdentifier("Sound")
            };
        }

        public ModelConditionFlag Required { get; private set; }
        public string Sound { get; private set; }
    }
}

using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public class W3dModelDraw : ObjectDrawModule
    {
        internal static W3dModelDraw ParseModel(IniParser parser)
        {
            return parser.ParseBlock(ModelFieldParseTable);
        }

        internal static readonly IniParseTable<W3dModelDraw> ModelFieldParseTable = new IniParseTable<W3dModelDraw>
        {
            { "AnimationsRequirePower", (parser, x) => x.AnimationsRequirePower = parser.ParseBoolean() },
            { "DefaultConditionState", (parser, x) => x.DefaultConditionState = ObjectConditionState.ParseDefault(parser) },
            { "ConditionState", (parser, x) => x.ConditionStates.Add(ObjectConditionState.Parse(parser)) },
            { "AliasConditionState", (parser, x) => x.ParseAliasConditionState(parser) },
            { "OkToChangeModelColor", (parser, x) => x.OkToChangeModelColor = parser.ParseBoolean() },
            { "ExtraPublicBone", (parser, x) => x.ExtraPublicBones.Add(parser.ParseBoneName()) },
        };

        public ObjectConditionState DefaultConditionState { get; private set; }
        public List<ObjectConditionState> ConditionStates { get; } = new List<ObjectConditionState>();

        public bool OkToChangeModelColor { get; private set; }
        public bool AnimationsRequirePower { get; private set; }
        public List<string> ExtraPublicBones { get; } = new List<string>();

        private void ParseAliasConditionState(IniParser parser)
        {
            if (ConditionStates.Count == 0)
            {
                throw new IniParseException("Cannot use AliasConditionState if there are no preceding ConditionStates", parser.CurrentPosition);
            }

            var lastConditionState = ConditionStates[ConditionStates.Count - 1];

            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var aliasedConditionState = lastConditionState.Clone(conditionFlags);

            ConditionStates.Add(aliasedConditionState);
        }
    }
}

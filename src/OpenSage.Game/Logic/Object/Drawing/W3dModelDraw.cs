using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public class W3dModelDraw : ObjectDrawModule
    {
        internal static W3dModelDraw ParseModel(IniParser parser) => parser.ParseBlock(ModelFieldParseTable);

        internal static readonly IniParseTable<W3dModelDraw> ModelFieldParseTable = new IniParseTable<W3dModelDraw>
        {
            { "DefaultConditionState", (parser, x) => parser.Temp = x.DefaultConditionState = ObjectConditionState.ParseDefault(parser) },
            {
                "ConditionState",
                (parser, x) =>
                {
                    var conditionState = ObjectConditionState.Parse(parser);
                    x.ConditionStates.Add(conditionState);
                    parser.Temp = conditionState;
                }
            },
            { "AliasConditionState", (parser, x) => x.ParseAliasConditionState(parser) },
            { "TransitionState", (parser, x) => x.TransitionStates.Add(TransitionState.Parse(parser)) },
            { "OkToChangeModelColor", (parser, x) => x.OkToChangeModelColor = parser.ParseBoolean() },
            { "ProjectileBoneFeedbackEnabledSlots", (parser, x) => x.ProjectileBoneFeedbackEnabledSlots = parser.ParseEnumBitArray<WeaponSlot>() },
            { "AnimationsRequirePower", (parser, x) => x.AnimationsRequirePower = parser.ParseBoolean() },
            { "MinLODRequired", (parser, x) => x.MinLodRequired = parser.ParseEnum<LevelOfDetail>() },
            { "ExtraPublicBone", (parser, x) => x.ExtraPublicBones.Add(parser.ParseBoneName()) },
            { "AttachToBoneInAnotherModule", (parser, x) => x.AttachToBoneInAnotherModule = parser.ParseBoneName() },
        };

        public ObjectConditionState DefaultConditionState { get; private set; }
        public List<ObjectConditionState> ConditionStates { get; } = new List<ObjectConditionState>();
        public List<TransitionState> TransitionStates { get; } = new List<TransitionState>();

        public bool OkToChangeModelColor { get; private set; }
        public BitArray<WeaponSlot> ProjectileBoneFeedbackEnabledSlots { get; private set; }
        public bool AnimationsRequirePower { get; private set; }

        /// <summary>
        /// Minimum level of detail required before this object appears in the game.
        /// </summary>
        public LevelOfDetail MinLodRequired { get; private set; }

        public List<string> ExtraPublicBones { get; } = new List<string>();
        public string AttachToBoneInAnotherModule { get; private set; }

        private void ParseAliasConditionState(IniParser parser)
        {
            var lastConditionState = parser.Temp as ObjectConditionState;
            if (lastConditionState == null)
            {
                throw new IniParseException("Cannot use AliasConditionState if there are no preceding ConditionStates", parser.CurrentPosition);
            }

            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var aliasedConditionState = lastConditionState.Clone(conditionFlags);

            ConditionStates.Add(aliasedConditionState);
        }
    }

    public enum LevelOfDetail
    {
        [IniEnum("MEDIUM")]
        Medium,
    }
}

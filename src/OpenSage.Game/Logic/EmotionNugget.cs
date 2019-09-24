﻿using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme)]
    public sealed class EmotionNugget : BaseAsset
    {
        internal static EmotionNugget Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("EmotionNugget", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<EmotionNugget> FieldParseTable = new IniParseTable<EmotionNugget>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<EmotionType>() },
            { "IgnoreIfUnitIdle", (parser, x) => x.IgnoreIfUnitIdle = parser.ParseBoolean() },
            { "IgnoreIfUnitBusy", (parser, x) => x.IgnoreIfUnitBusy = parser.ParseBoolean() },
            { "Duration", (parser, x) => x.Duration = parser.ParseInteger() },
            { "InactiveDuration", (parser, x) => x.InactiveDuration = parser.ParseInteger() },
            { "InactiveDurationSameObject", (parser, x) => x.InactiveDurationSameObject = parser.ParseInteger() },
            { "InactiveDurationSameType", (parser, x) => x.InactiveDurationSameType = parser.ParseInteger() },
            { "OnlyIfEnemyThreatBelow", (parser, x) => x.OnlyIfEnemyThreatBelow = parser.ParseInteger() },
            { "StartFXList", (parser, x) => x.StartFXList = parser.ParseAssetReference() },
            { "UpdateFXList", (parser, x) => x.UpdateFXList = parser.ParseAssetReference() },
            { "EndFXList", (parser, x) => x.EndFXList = parser.ParseAssetReference() },
            { "AttributeModifierWhileEmotionActive", (parser, x) => x.AttributeModifierWhileEmotionActive = parser.ParseBoolean() },
            { "AttributeStartDelay", (parser, x) => x.AttributeStartDelay = parser.ParseInteger() },
            { "AILockDuration", (parser, x) => x.AILockDuration = parser.ParseInteger() },
            { "AIState", (parser, x) => x.AIState = parser.ParseEnum<EmotionAIType>() },
            { "ModelConditions", (parser, x) => x.ModelConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "ModelConditionsClear", (parser, x) => x.ModelConditionsClear = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "PreventPlayerCommands", (parser, x) => x.PreventPlayerCommands = parser.ParseBoolean() }
        };

        public EmotionType Type { get; private set; }
        public bool IgnoreIfUnitIdle { get; private set; }
        public bool IgnoreIfUnitBusy { get; private set; }
        public int Duration { get; private set; }
        public int InactiveDuration { get; private set; }
        public int InactiveDurationSameObject { get; private set; }
        public int InactiveDurationSameType { get; private set; }
        public int OnlyIfEnemyThreatBelow { get; private set; }
        public string StartFXList { get; private set; }
        public string UpdateFXList { get; private set; }
        public string EndFXList { get; private set; }
        public bool AttributeModifierWhileEmotionActive { get; private set; }
        public int AttributeStartDelay { get; private set; }
        public int AILockDuration { get; private set; }
        public EmotionAIType AIState { get; private set; }
        public BitArray<ModelConditionFlag> ModelConditions { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> ModelConditionsClear { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool PreventPlayerCommands { get; private set; }
    }

    public enum EmotionType
    {
        None = 0,

        [IniEnum("TAUNT")]
        Taunt,

        [IniEnum("CHEER")]
        Cheer,

        [IniEnum("HERO_CHEER")]
        HeroCheer,

        [IniEnum("POINT")]
        Point,

        [IniEnum("FEAR")]
        Fear,

        [IniEnum("UNCONTROLLABLE_FEAR")]
        UncontrollableFear,

        [IniEnum("TERROR")]
        Terror,

        [IniEnum("DOOM")]
        Doom,

        [IniEnum("ALERT")]
        Alert,

        [IniEnum("QUARRELSOME")]
        Quarrelsome,

        [IniEnum("OVERRIDE"), AddedIn(SageGame.Bfme)]
        Override,

        [IniEnum("CHEER_FOR_ABOUT_TO_CRUSH"), AddedIn(SageGame.Bfme2)]
        CheerForAboutToCrush,

        [IniEnum("BRACE_FOR_BEING_CRUSHED"), AddedIn(SageGame.Bfme2)]
        BraceForBeingCrushed,
    }

    public enum EmotionAIType
    {
        None,

        [IniEnum("BACK_AWAY")]
        BackAway,

        [IniEnum("AVOID_SCARER")]
        AvoidScarer,

        [IniEnum("IDLE")]
        Idle,

        [IniEnum("RUN_AWAY_PANIC")]
        RunAwayPanic,

        [IniEnum("FACE_OBJECT")]
        FaceObject,

        [IniEnum("QUARREL")]
        Quarrel,
    }
}

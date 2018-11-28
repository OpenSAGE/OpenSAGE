using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class AnimationState 
    {
        internal static AnimationState Parse(IniParser parser)
        {
            var stateTypeFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var result = parser.ParseBlock(FieldParseTable);

            result.TypeFlags = stateTypeFlags;

            return result;
        }

        internal static readonly IniParseTable<AnimationState> FieldParseTable = new IniParseTable<AnimationState>
        {
            { "Animation", (parser, x) => x.Animations.Add(Animation.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
            { "Flags", (parser, x) => x.Flags = parser.ParseEnumFlags<AnimationFlags>() },
            { "BeginScript", (parser, x) => x.Script = IniScript.Parse(parser) },
            { "StateName", (parser, x) => x.StateName = parser.ParseString() },
            { "FrameForPristineBonePositions", (parser, x) => x.FrameForPristineBonePositions = parser.ParseInteger() },
            { "SimilarRestart", (parser, x) => x.SimilarRestart = parser.ParseBoolean() },
            { "EnteringStateFX", (parser, x) => x.EnteringStateFX = parser.ParseAssetReference() },
            { "FXEvent", (parser, x) => x.FXEvents.Add(FXEvent.Parse(parser)) },
            { "ShareAnimation", (parser, x) => x.ShareAnimation = parser.ParseBoolean() }
        };

        public BitArray<ModelConditionFlag> TypeFlags { get; private set; }

        public List<Animation> Animations { get; private set; } = new List<Animation>();
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();
        public AnimationFlags Flags { get; private set; }
        public IniScript Script { get; private set; }
        public string StateName { get; private set; }
        public int FrameForPristineBonePositions { get; private set; }
        public bool SimilarRestart { get; private set; }
        public string EnteringStateFX { get; private set; }
        public List<FXEvent> FXEvents { get; private set; } = new List<FXEvent>();
        public bool ShareAnimation { get; private set; }
    }

    public sealed class Animation
    {
        internal static Animation Parse(IniParser parser)
        {
            var token = parser.GetNextTokenOptional();
            var animationType = "";
            if (token.HasValue)
            {
                animationType = token.Value.Text;
            }

            var result = parser.ParseBlock(FieldParseTable);

            result.AnimationType = animationType;

            return result;
        }

        internal static readonly IniParseTable<Animation> FieldParseTable = new IniParseTable<Animation>
        {
            { "AnimationName", (parser, x) => x.AnimationName = parser.ParseString() },
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationPriority", (parser, x) => x.AnimationPriority = parser.ParseInteger() },
            { "UseWeaponTiming", (parser, x) => x.UseWeaponTiming = parser.ParseBoolean() },
            { "AnimationBlendTime", (parser, x) => x.AnimationBlendTime = parser.ParseInteger() },
            { "AnimationSpeedFactorRange", (parser, x) => x.AnimationSpeedFactorRange = FloatRange.Parse(parser) },
            { "Distance", (parser, x) => x.Distance = parser.ParseFloat() },
            { "AnimationMustCompleteBlend", (parser, x) => x.AnimationMustCompleteBlend = parser.ParseBoolean() },
            { "FadeBeginFrame", (parser, x) => x.FadeBeginFrame = parser.ParseFloat() },
            { "FadeEndFrame", (parser, x) => x.FadeEndFrame = parser.ParseFloat() },
            { "FadingIn", (parser, x) => x.FadingIn = parser.ParseBoolean() }
        };

        public string AnimationType { get; private set; }

        public string AnimationName { get; private set; }
        public AnimationMode AnimationMode { get; private set; }
        public int AnimationPriority { get; private set; }
        public bool UseWeaponTiming { get; private set; }
        public int AnimationBlendTime { get; private set; }
        public FloatRange AnimationSpeedFactorRange { get; private set; }
        public float Distance { get; private set; }
        public bool AnimationMustCompleteBlend { get; private set; }
        public float FadeBeginFrame { get; private set; }
        public float FadeEndFrame { get; private set; }
        public bool FadingIn { get; private set; }
    }

    public sealed class FXEvent
    {
        internal static FXEvent Parse(IniParser parser)
        {
            return new FXEvent()
            {
                Frame = parser.ParseAttributeInteger("Frame"),
                FX = parser.ParseAttributeIdentifier("Name")
            };
        }

        public int Frame { get; private set; }
        public string FX { get; private set; }
    }
}

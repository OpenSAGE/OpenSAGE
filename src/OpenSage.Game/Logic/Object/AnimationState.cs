using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class AnimationState : IConditionState
    {
        internal static AnimationState Parse(IniParser parser)
        {
            var stateTypeFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var result = parser.ParseBlock(FieldParseTable);

            result.ConditionFlags = stateTypeFlags;

            return result;
        }

        internal static readonly IniParseTable<AnimationState> FieldParseTable = new IniParseTable<AnimationState>
        {
            { "Animation", (parser, x) => x.Animations.Add(AnimationStateAnimation.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
            { "Flags", (parser, x) => x.Flags = parser.ParseEnumFlags<AnimationFlags>() },
            { "BeginScript", (parser, x) => x.Script = IniScript.Parse(parser) },
            { "StateName", (parser, x) => x.StateName = parser.ParseString() },
            { "FrameForPristineBonePositions", (parser, x) => x.FrameForPristineBonePositions = parser.ParseInteger() },
            { "SimilarRestart", (parser, x) => x.SimilarRestart = parser.ParseBoolean() },
            { "EnteringStateFX", (parser, x) => x.EnteringStateFX = parser.ParseAssetReference() },
            { "FXEvent", (parser, x) => x.FXEvents.Add(FXEvent.Parse(parser)) },
            { "ShareAnimation", (parser, x) => x.ShareAnimation = parser.ParseBoolean() },
            { "AllowRepeatInRandomPick", (parser, x) => x.AllowRepeatInRandomPick = parser.ParseBoolean() },
            { "LuaEvent", (parser, x) => x.LuaEvents.Add(LuaEvent.Parse(parser)) }
        };

        public BitArray<ModelConditionFlag> ConditionFlags { get; private set; }

        public List<AnimationStateAnimation> Animations { get; private set; } = new List<AnimationStateAnimation>();
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();
        public AnimationFlags Flags { get; private set; }
        public IniScript Script { get; private set; }
        public string StateName { get; private set; }
        public int FrameForPristineBonePositions { get; private set; }
        public bool SimilarRestart { get; private set; }
        public string EnteringStateFX { get; private set; }
        public List<FXEvent> FXEvents { get; private set; } = new List<FXEvent>();
        public bool ShareAnimation { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AllowRepeatInRandomPick { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<LuaEvent> LuaEvents { get; } = new List<LuaEvent>();
    }

    public sealed class AnimationStateAnimation
    {
        internal static AnimationStateAnimation Parse(IniParser parser)
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

        internal static readonly IniParseTable<AnimationStateAnimation> FieldParseTable = new IniParseTable<AnimationStateAnimation>
        {
            { "AnimationName", (parser, x) => x.Animations = parser.ParseAnimationReferenceArray() },
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationPriority", (parser, x) => x.AnimationPriority = parser.ParseInteger() },
            { "UseWeaponTiming", (parser, x) => x.UseWeaponTiming = parser.ParseBoolean() },
            { "AnimationBlendTime", (parser, x) => x.AnimationBlendTime = parser.ParseInteger() },
            { "AnimationSpeedFactorRange", (parser, x) => x.AnimationSpeedFactorRange = parser.ParseFloatRange() },
            { "Distance", (parser, x) => x.Distance = parser.ParseFloat() },
            { "AnimationMustCompleteBlend", (parser, x) => x.AnimationMustCompleteBlend = parser.ParseBoolean() },
            { "FadeBeginFrame", (parser, x) => x.FadeBeginFrame = parser.ParseFloat() },
            { "FadeEndFrame", (parser, x) => x.FadeEndFrame = parser.ParseFloat() },
            { "FadingIn", (parser, x) => x.FadingIn = parser.ParseBoolean() }
        };

        public string AnimationType { get; private set; }
        public LazyAssetReference<Graphics.Animation.W3DAnimation>[] Animations { get; private set; }
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
        internal static FXEvent Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<FXEvent> FieldParseTable = new IniParseTable<FXEvent>
        {
            { "Frame", (parser, x) => x.Frame = parser.ParseInteger() },
            { "Name", (parser, x) => x.Name = parser.ParseIdentifier() },
            { "FrameStop", (parser, x) => x.FrameStop = parser.ParseInteger() },
            { "FireWhenSkipped", (parser, x) => x.FireWhenSkipped = true },
        };

        public int Frame { get; private set; }
        public string Name { get; private set; }
        public int FrameStop { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool FireWhenSkipped { get; private set; } = false;
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class LuaEvent
    {
        internal static LuaEvent Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<LuaEvent> FieldParseTable = new IniParseTable<LuaEvent>
        {
            { "Frame", (parser, x) => x.Frame = parser.ParseInteger() },
            { "Data", (parser, x) => x.Data = parser.ParseString() },
            { "OnStateEnter", (parser, x) => x.OnStateEnter = true },
        };

        public int Frame { get; private set; }
        public string Data { get; private set; }
        public bool OnStateEnter { get; private set; }
    }
}

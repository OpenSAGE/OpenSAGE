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
             var stateTypeFlags = parser.ParseEnumBitArray<AnimationStateTypeFlags>();

            var result = parser.ParseBlock(FieldParseTable);

            result.TypeFlags = stateTypeFlags;

            return result;
        }

        internal static readonly IniParseTable<AnimationState> FieldParseTable = new IniParseTable<AnimationState>
        {
            { "Animation", (parser, x) => x.Animations.Add(Animation.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
            { "Flags", (parser, x) => x.Flags = parser.ParseEnumBitArray<AnimationFlags>() },
            //{ "BeginScript", (parser, x) => x.Flags = parser.ParseEnumBitArray<AnimationFlags>() }
        };

        public BitArray<AnimationStateTypeFlags> TypeFlags { get; private set; }

        public List<Animation> Animations { get; private set; } = new List<Animation>();
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();
        public BitArray<AnimationFlags> Flags { get; private set; }
    }

    public sealed class Animation
    {
        internal static Animation Parse(IniParser parser)
        {
            var animationType = parser.ParseEnum<AnimationType>();

            var result = parser.ParseBlock(FieldParseTable);

            result.AnimationType = animationType;

            return result;
        }

        internal static readonly IniParseTable<Animation> FieldParseTable = new IniParseTable<Animation>
        {
            { "AnimationName", (parser, x) => x.AnimationName = parser.ParseString() },
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationPriority", (parser, x) => x.AnimationPriority = parser.ParseInteger() },
        };

        public AnimationType AnimationType { get; private set; }

        public string AnimationName { get; private set; }
        public AnimationMode AnimationMode { get; private set; }
        public int AnimationPriority { get; private set; }
    }

    

    public enum AnimationMode
    {
        [IniEnum("LOOP")]
        Loop,
        [IniEnum("ONCE")]
        Once
    }
}

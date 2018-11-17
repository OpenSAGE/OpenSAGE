using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public class AnimationState 
    {
        internal static AnimationState Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        internal static readonly IniParseTable<AnimationState> FieldParseTable = new IniParseTable<AnimationState>
        {
            { "Animation", (parser, x) => x.Animations.Add(Animation.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
        };

        public List<Animation> Animations { get; private set; } = new List<Animation>();
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();
    }

    public sealed class Animation
    {
        internal static Animation Parse(IniParser parser)
        {
            return new Animation
            {
                AnimationName = parser.ParseAnimationName(),
                AnimationMode = parser.ParseString()
            };
        }

        public string AnimationName { get; private set; }
        public string AnimationMode { get; private set; }
    }
}

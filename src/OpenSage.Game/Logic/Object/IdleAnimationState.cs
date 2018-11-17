using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class IdleAnimationState : AnimationState 
    {
        internal static IdleAnimationState Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        internal static readonly IniParseTable<IdleAnimationState> FieldParseTable = new IniParseTable<IdleAnimationState>
        {
            { "Animation", (parser, x) => x.Animations.Add(Animation.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.ParticleSysBones.Add(ParticleSysBone.Parse(parser)) },
        };

        public List<Animation> Animations { get; private set; } = new List<Animation>();
        public List<ParticleSysBone> ParticleSysBones { get; private set; } = new List<ParticleSysBone>();
    }
}

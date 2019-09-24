﻿using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.LivingWorld.AutoResolve
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveCombatChain : BaseAsset
    {
        internal static AutoResolveCombatChain Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("AutoResolveCombatChain", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<AutoResolveCombatChain> FieldParseTable = new IniParseTable<AutoResolveCombatChain>
        {
            { "Target", (parser, x) => x.TargetPriorities.Add(TargetPriority.Parse(parser)) },
        };

        public List<TargetPriority> TargetPriorities { get; } = new List<TargetPriority>();
    }

    public class TargetPriority
    {
        internal static TargetPriority Parse(IniParser parser)
        {
            return new TargetPriority
            {
                Target = parser.ParseAttributeIdentifier("Target"),
                Priority = parser.ParseAttributeInteger("Priority")
            };
        }

        public string Target { get; private set; }
        public int Priority { get; private set; }
    }
}

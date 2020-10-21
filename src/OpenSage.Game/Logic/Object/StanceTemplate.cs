﻿using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class StanceTemplate : BaseAsset
    {
        internal static StanceTemplate Parse(IniParser parser)
        {
            StanceAttributeModifier.AttributeModifiers.Clear();
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("StanceTemplate", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<StanceTemplate> FieldParseTable = new IniParseTable<StanceTemplate>
        {
            { "Stance", (parser, x) => x.Stances.Add(Stance.Parse(parser)) },
        };

        public List<Stance> Stances { get; } = new List<Stance>();
    }

    public sealed class Stance
    {
        internal static Stance Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Stance> FieldParseTable = new IniParseTable<Stance>
        {
            { "AttributeModifier", (parser, x) => x.AttributeModifier = StanceAttributeModifier.Parse(parser) },
            { "MeleeBehavior", (parser, x) => x.MeleeBehavior = MeleeBehavior.Parse(parser) },
        };

        public string Name { get; private set; }
        public StanceAttributeModifier AttributeModifier { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public MeleeBehavior MeleeBehavior { get; private set; }
    }

    public sealed class StanceAttributeModifier
    {
        internal static Dictionary<string, StanceAttributeModifier> AttributeModifiers { get; } = new Dictionary<string, StanceAttributeModifier>();

        internal static StanceAttributeModifier Parse(IniParser parser)
        {
            var name = parser.ParseString();
            if (IsOnlyAReference(name))
            {
                return AttributeModifiers[name];
            }

            var result = parser.ParseBlock(FieldParseTable);
            result.Name = name;
            AttributeModifiers.Add(name, result);
            return result;
        }

        private static bool IsOnlyAReference(string name)
        {
            return AttributeModifiers.ContainsKey(name);
        }

        private static readonly IniParseTable<StanceAttributeModifier> FieldParseTable = new IniParseTable<StanceAttributeModifier>
        {
            { "MeleeBehavior", (parser, x) => x.MeleeBehavior = parser.ParseString() },
        };

        public string Name { get; private set; }
        public string MeleeBehavior { get; private set; }
    }
}

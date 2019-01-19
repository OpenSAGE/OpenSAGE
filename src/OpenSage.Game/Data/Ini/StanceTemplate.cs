using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class StanceTemplate
    {
        internal static StanceTemplate Parse(IniParser parser)
        {
            AttributeModifier.AttributeModifiers.Clear();
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<StanceTemplate> FieldParseTable = new IniParseTable<StanceTemplate>
        {
            { "Stance", (parser, x) => x.Stances.Add(Stance.Parse(parser)) },
        };

        public string Name { get; private set; }
        public List<Stance> Stances { get; } = new List<Stance>();
    }

    public sealed class Stance
    {
        
        internal static Stance Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Stance> FieldParseTable = new IniParseTable<Stance>
        {
            { "AttributeModifier", (parser, x) => x.AttributeModifier = AttributeModifier.Parse(parser) },
        };

        public string Name { get; private set; }
        public AttributeModifier AttributeModifier { get; private set; }
    }

    public sealed class AttributeModifier
    {
        internal static Dictionary<string, AttributeModifier> AttributeModifiers { get; } = new Dictionary<string, AttributeModifier>();

        internal static AttributeModifier Parse(IniParser parser)
        {
            var name = parser.ParseString();
            if (IsOnlyAReference(name)) return AttributeModifiers[name];

            var result = parser.ParseBlock(FieldParseTable);
            result.Name = name;
            AttributeModifiers.Add(name, result);
            return result;
        }

        private static bool IsOnlyAReference(string name)
        {
            return AttributeModifiers.ContainsKey(name);
        }

        private static readonly IniParseTable<AttributeModifier> FieldParseTable = new IniParseTable<AttributeModifier>
        {
            { "MeleeBehavior", (parser, x) => x.MeleeBehavior = parser.ParseString() },
        };

        public string Name { get; private set; }
        public string MeleeBehavior { get; private set; }
    }
}

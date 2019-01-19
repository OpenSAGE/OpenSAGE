using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class SkirmishAIData
    {
        internal static SkirmishAIData Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<SkirmishAIData> FieldParseTable = new IniParseTable<SkirmishAIData>
        {
            { "CombatChainDefinition", (parser, x) => x.CombatChainDefinitions.Add(CombatChainDefinition.Parse(parser)) },
            { "AnyTypeTemplateDisabledSlots", (parser, x) => x.AnyTypeTemplateDisabledSlots = parser.ParseInteger() },
        };

        public string Name { get; private set; }
        public List<CombatChainDefinition> CombatChainDefinitions { get; } = new List<CombatChainDefinition>();
        public int AnyTypeTemplateDisabledSlots { get; private set; }
    }

    public sealed class CombatChainDefinition
    {
        internal static CombatChainDefinition Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<CombatChainDefinition> FieldParseTable = new IniParseTable<CombatChainDefinition>
        {
            { "Unit", (parser, x) => x.Unit = parser.ParseEnum<ObjectKinds>() },
            { "TargetTypes", (parser, x) => x.TargetTypes = parser.ParseEnumBitArray<ObjectKinds>() },
            { "TargetPriorityModifiers", (parser, x) => x.TargetPriorityModifiers = parser.ParseFloatArray() }
            
        };

        public string Name { get; private set; }
        public ObjectKinds Unit { get; private set; }
        public BitArray<ObjectKinds> TargetTypes { get; private set; }
        public float[] TargetPriorityModifiers { get; private set; }
    }
}

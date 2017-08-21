using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyCrateCollide : ObjectBehavior
    {
        internal static VeterancyCrateCollide Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<VeterancyCrateCollide> FieldParseTable = new IniParseTable<VeterancyCrateCollide>
        {
            { "RequiredKindOf", (parser, x) => x.RequiredKindOf = parser.ParseEnum<ObjectKinds>() },
            { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
            { "EffectRange", (parser, x) => x.EffectRange = parser.ParseInteger() },
            { "AddsOwnerVeterancy", (parser, x) => x.AddsOwnerVeterancy = parser.ParseBoolean() },
            { "IsPilot", (parser, x) => x.IsPilot = parser.ParseBoolean() }
        };

        public ObjectKinds RequiredKindOf { get; private set; }
        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int EffectRange { get; private set; }
        public bool AddsOwnerVeterancy { get; private set; }
        public bool IsPilot { get; private set; }
    }
}

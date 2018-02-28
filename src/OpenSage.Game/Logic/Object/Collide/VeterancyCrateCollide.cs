using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyCrateCollideModuleData : CrateCollideModuleData
    {
        internal static VeterancyCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<VeterancyCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<VeterancyCrateCollideModuleData>
            {
                { "EffectRange", (parser, x) => x.EffectRange = parser.ParseInteger() },
                { "AddsOwnerVeterancy", (parser, x) => x.AddsOwnerVeterancy = parser.ParseBoolean() },
                { "IsPilot", (parser, x) => x.IsPilot = parser.ParseBoolean() },
                { "ExecuteFX", (parser, x) => x.ExecuteFX = parser.ParseAssetReference() },
                { "AffectsUpToLevel", (parser, x) => x.AffectsUpToLevel = parser.ParseInteger() },
            });

       
        public int EffectRange { get; private set; }
        public bool AddsOwnerVeterancy { get; private set; }
        public bool IsPilot { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ExecuteFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int AffectsUpToLevel { get; private set; }
    }
}

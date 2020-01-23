using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class OilSpillUpdateModuleData : UpdateModuleData
    {
        internal static OilSpillUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<OilSpillUpdateModuleData> FieldParseTable = new IniParseTable<OilSpillUpdateModuleData>
        {
            { "FireWeaponNugget", (parser, x) => x.FireWeaponNugget = WeaponNugget.Parse(parser) },
            { "BreadcrumbName", (parser, x) => x.BreadcrumbName = parser.ParseString() },
            { "IgnitionWeaponSpacing", (parser, x) => x.IgnitionWeaponSpacing = parser.ParseFloat() },
            { "AliveOnly", (parser, x) => x.AliveOnly = parser.ParseBoolean() },
            { "OilSpillFX", (parser, x) => x.OilSpillFX = parser.ParseAssetReference() }
        };

        public WeaponNugget FireWeaponNugget { get; private set; }
        public string BreadcrumbName { get; private set; }
        public float IgnitionWeaponSpacing { get; private set; }
        public bool AliveOnly { get; private set; }
        public string OilSpillFX { get; private set; }
    }
}

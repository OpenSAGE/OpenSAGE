using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class PorcupineFormationBodyModuleData : ActiveBodyModuleData
    {
        internal static new PorcupineFormationBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<PorcupineFormationBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<PorcupineFormationBodyModuleData>
            {
                { "DamageWeaponTemplate", (parser, x) => x.DamageWeaponTemplate = parser.ParseAssetReference() },
                { "CrushDamageWeaponTemplate", (parser, x) => x.CrushDamageWeaponTemplate = parser.ParseAssetReference() },
            });

        public string DamageWeaponTemplate { get; internal set; }
        public string CrushDamageWeaponTemplate { get; internal set; }
     }
}

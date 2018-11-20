using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Used by objects with STRUCTURE and IMMOBILE KindOfs defined.
    /// </summary>
    public sealed class StructureBodyModuleData : ActiveBodyModuleData
    {
        internal static new StructureBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StructureBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<StructureBodyModuleData>()
            {
                { "GrabFX", (parser, x) => x.GrabFX = parser.ParseAssetReference() },
                { "GrabDamage", (parser, x) => x.GrabDamage = parser.ParseInteger() },
            });

        [AddedIn(SageGame.Bfme)]
        public string GrabFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int GrabDamage { get; private set; } 
     }
}

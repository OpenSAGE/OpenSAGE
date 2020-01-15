using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class SpawnAndFadeNugget : WeaponEffectNuggetData
    {
        internal static SpawnAndFadeNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpawnAndFadeNugget> FieldParseTable = WeaponEffectNuggetData.FieldParseTable
            .Concat(new IniParseTable<SpawnAndFadeNugget>
            {
                { "ObjectTargetFilter", (parser, x) => x.ObjectTargetFilter = ObjectFilter.Parse(parser) },
                { "SpawnedObjectName", (parser, x) => x.SpawnedObjectName = parser.ParseAssetReference() },
                { "SpawnOffset", (parser, x) => x.SpawnOffset = parser.ParseVector3() }
            });

        public ObjectFilter ObjectTargetFilter {get; private set; } 
        public string SpawnedObjectName {get; private set; } 
        public Vector3 SpawnOffset {get; private set; } 
    }
}

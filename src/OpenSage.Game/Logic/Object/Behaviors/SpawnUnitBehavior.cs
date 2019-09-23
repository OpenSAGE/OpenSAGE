using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class SpawnUnitBehaviorModuleData : BehaviorModuleData
    {
        internal static SpawnUnitBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpawnUnitBehaviorModuleData> FieldParseTable = new IniParseTable<SpawnUnitBehaviorModuleData>
            {
                { "UnitName", (parser, x) => x.UnitName = parser.ParseString() },
                { "UnitCommand", (parser, x) => x.UnitCommand = parser.ParseAssetReference() },
                { "SpawnOnce", (parser, x) => x.SpawnOnce = parser.ParseBoolean() }
            };

        public string UnitName { get; private set; }
        public string UnitCommand { get; private set; }
        public bool SpawnOnce { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class CivilianSpawnCollideBehaviorModuleData : BehaviorModuleData
    {
        internal static CivilianSpawnCollideBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<CivilianSpawnCollideBehaviorModuleData> FieldParseTable = new IniParseTable<CivilianSpawnCollideBehaviorModuleData>
        {
            { "DeleteObjectFilter", (parser, x) => x.DeleteObjectFilter = ObjectFilter.Parse(parser) },
        };

        public ObjectFilter DeleteObjectFilter { get; internal set; }
    }
}

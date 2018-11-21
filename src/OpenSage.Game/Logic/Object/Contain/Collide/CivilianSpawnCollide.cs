using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class CivilianSpawnCollideModuleData : BehaviorModuleData
    {
        internal static CivilianSpawnCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<CivilianSpawnCollideModuleData> FieldParseTable = new IniParseTable<CivilianSpawnCollideModuleData>
        {
            { "DeleteObjectFilter", (parser, x) => x.DeleteObjectFilter = ObjectFilter.Parse(parser) },
        };

        public ObjectFilter DeleteObjectFilter { get; internal set; }
    }
}

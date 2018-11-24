using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class BodyModuleData : BehaviorModuleData
    {
        internal static BodyModuleData ParseBody(IniParser parser) => ParseModule(parser, BodyParseTable);

        private static readonly Dictionary<string, Func<IniParser, BodyModuleData>> BodyParseTable = new Dictionary<string, Func<IniParser, BodyModuleData>>
        {
            { "ActiveBody", ActiveBodyModuleData.Parse },
            { "DelayedDeathBody", DelayedDeathBodyModuleData.Parse },
            { "DetachableRiderBody", DetachableRiderBodyModuleData.Parse },
            { "HighlanderBody", HighlanderBodyModuleData.Parse },
            { "HiveStructureBody", HiveStructureBodyModuleData.Parse },
            { "ImmortalBody", ImmortalBodyModuleData.Parse },
            { "InactiveBody", InactiveBodyModuleData.Parse },
            { "StructureBody", StructureBodyModuleData.Parse },
            { "UndeadBody", UndeadBodyModuleData.Parse },
        };
    }
}

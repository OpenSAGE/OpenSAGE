using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class ObjectBody : ObjectModule
    {
        internal static ObjectBody ParseBody(IniParser parser) => ParseModule(parser, BodyParseTable);

        private static readonly Dictionary<string, Func<IniParser, ObjectBody>> BodyParseTable = new Dictionary<string, Func<IniParser, ObjectBody>>
        {
            { "ActiveBody", ActiveBody.Parse },
            { "HighlanderBody", HighlanderBody.Parse },
            { "HiveStructureBody", HiveStructureBody.Parse },
            { "ImmortalBody", ImmortalBody.Parse },
            { "InactiveBody", InactiveBody.Parse },
            { "StructureBody", StructureBody.Parse },
        };
    }
}

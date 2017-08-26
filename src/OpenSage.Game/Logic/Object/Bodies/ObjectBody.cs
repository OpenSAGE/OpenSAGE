using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
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

        internal static readonly IniParseTable<ObjectBody> BodyFieldParseTable = new IniParseTable<ObjectBody>
        {
            { "SubdualDamageCap", (parser, x) => x.SubdualDamageCap = parser.ParseInteger() },
            { "SubdualDamageHealRate", (parser, x) => x.SubdualDamageHealRate = parser.ParseInteger() },
            { "SubdualDamageHealAmount", (parser, x) => x.SubdualDamageHealAmount = parser.ParseInteger() }
        };

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageCap { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageHealRate { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageHealAmount { get; private set; }
    }
}

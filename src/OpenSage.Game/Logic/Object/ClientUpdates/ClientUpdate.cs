using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class ClientUpdate : ObjectModule
    {
        internal static ClientUpdate ParseClientUpdate(IniParser parser) => ParseModule(parser, ClientUpdateParseTable);

        private static readonly Dictionary<string, Func<IniParser, ClientUpdate>> ClientUpdateParseTable = new Dictionary<string, Func<IniParser, ClientUpdate>>
        {
            { "AnimatedParticleSysBoneClientUpdate", AnimatedParticleSysBoneClientUpdate.Parse },
        };
    }
}

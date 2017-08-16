using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class ObjectModule
    {
        internal static T ParseModule<T>(IniParser parser, Dictionary<string, Func<IniParser, T>> moduleParseTable)
            where T : ObjectModule
        {
            var moduleTypePosition = parser.CurrentPosition;
            var moduleType = parser.ParseIdentifier();
            var tag = parser.ParseIdentifier();

            if (!moduleParseTable.TryGetValue(moduleType, out var moduleParser))
            {
                throw new IniParseException($"Unknown module type: {moduleType}", moduleTypePosition);
            }

            var result = moduleParser(parser);

            result.Tag = tag;

            return result;
        }

        public string Tag { get; protected set; }
    }
}

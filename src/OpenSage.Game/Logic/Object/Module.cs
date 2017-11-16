using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class ModuleData
    {
        internal static T ParseModule<T>(IniParser parser, Dictionary<string, Func<IniParser, T>> moduleParseTable)
            where T : ModuleData
        {
            var moduleType = parser.GetNextToken();
            var tag = parser.GetNextToken();

            if (!moduleParseTable.TryGetValue(moduleType.Text, out var moduleParser))
            {
                throw new IniParseException($"Unknown module type: {moduleType.Text}", moduleType.Position);
            }

            var result = moduleParser(parser);

            result.Tag = tag.Text;

            return result;
        }

        public string Tag { get; protected set; }
    }
}

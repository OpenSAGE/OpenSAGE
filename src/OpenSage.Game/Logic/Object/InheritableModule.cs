using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Modules in here are not removed by default when copied from the default object.
    /// </summary>
    public sealed class InheritableModule
    {
        internal static InheritableModule Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InheritableModule> FieldParseTable = new IniParseTable<InheritableModule>
        {
            { "Behavior", (parser, x) => x.Behaviors.Add(ObjectBehavior.ParseBehavior(parser)) },
        };

        public List<ObjectBehavior> Behaviors { get; } = new List<ObjectBehavior>();
    }
}

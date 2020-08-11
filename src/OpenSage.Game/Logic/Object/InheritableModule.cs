using OpenSage.Data.Ini;

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
            { "Behavior", (parser, x) => x.Module = BehaviorModuleData.ParseBehavior(parser) },
        };

        public BehaviorModuleData Module { get; private set; }
    }
}

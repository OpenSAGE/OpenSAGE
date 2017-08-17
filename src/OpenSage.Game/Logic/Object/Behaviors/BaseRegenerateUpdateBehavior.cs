using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Forces object to auto-repair itself over time. Parameters are defined in GameData.INI 
    /// through <see cref="GameData.BaseRegenHealthPercentPerSecond"/> and
    /// <see cref="GameData.BaseRegenDelay"/>.
    /// </summary>
    public sealed class BaseRegenerateUpdateBehavior : ObjectBehavior
    {
        internal static BaseRegenerateUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BaseRegenerateUpdateBehavior> FieldParseTable = new IniParseTable<BaseRegenerateUpdateBehavior>();
    }
}

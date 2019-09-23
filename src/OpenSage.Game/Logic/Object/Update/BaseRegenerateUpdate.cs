using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Forces object to auto-repair itself over time. Parameters are defined in GameData.INI 
    /// through <see cref="GameData.BaseRegenHealthPercentPerSecond"/> and
    /// <see cref="GameData.BaseRegenDelay"/>.
    /// </summary>
    public sealed class BaseRegenerateUpdateModuleData : UpdateModuleData
    {
        internal static BaseRegenerateUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BaseRegenerateUpdateModuleData> FieldParseTable = new IniParseTable<BaseRegenerateUpdateModuleData>();
    }
}

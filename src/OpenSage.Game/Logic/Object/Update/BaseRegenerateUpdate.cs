using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class BaseRegenerateUpdate : UpdateModule
    {
        // TODO

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }

    /// <summary>
    /// Forces object to auto-repair itself over time. Parameters are defined in GameData.INI 
    /// through <see cref="GameData.BaseRegenHealthPercentPerSecond"/> and
    /// <see cref="GameData.BaseRegenDelay"/>.
    /// </summary>
    public sealed class BaseRegenerateUpdateModuleData : UpdateModuleData
    {
        internal static BaseRegenerateUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BaseRegenerateUpdateModuleData> FieldParseTable = new IniParseTable<BaseRegenerateUpdateModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new BaseRegenerateUpdate();
        }
    }
}

using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    internal sealed class StealthUpgrade : UpgradeModule
    {
        public StealthUpgrade(GameObject gameObject, StealthUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

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
    /// Eenables use of <see cref="StealthUpdateModuleData"/> module on this object. Requires 
    /// <see cref="StealthUpdateModuleData.InnateStealth"/> = No defined in the <see cref="StealthUpdateModuleData"/> 
    /// module.
    /// </summary>
    public sealed class StealthUpgradeModuleData : UpgradeModuleData
    {
        internal static StealthUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StealthUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<StealthUpgradeModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new StealthUpgrade(gameObject, this);
        }
    }
}

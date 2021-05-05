using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    internal sealed class RadarUpgrade : UpgradeModule
    {
        internal RadarUpgrade(GameObject gameObject, RadarUpgradeModuleData moduleData)
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
    /// Triggers use of <see cref="RadarUpdateModuleData"/> module on this object if present and enables the 
    /// Radar in the command bar.
    /// </summary>
    public sealed class RadarUpgradeModuleData : UpgradeModuleData
    {
        internal static RadarUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RadarUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<RadarUpgradeModuleData>
            {
                { "DisableProof", (parser, x) => x.DisableProof = parser.ParseBoolean() }
            });

        public bool DisableProof { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new RadarUpgrade(gameObject, this);
        }
    }
}

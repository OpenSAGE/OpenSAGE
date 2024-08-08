using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    internal sealed class ArmorUpgrade : UpgradeModule
    {
        private readonly ArmorUpgradeModuleData _moduleData;

        internal ArmorUpgrade(GameObject gameObject, ArmorUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    /// <summary>
    /// Triggers use of PLAYER_UPGRADE ArmorSet on this object.
    /// </summary>
    public sealed class ArmorUpgradeModuleData : UpgradeModuleData
    {
        internal static ArmorUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ArmorUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ArmorUpgradeModuleData>()
            {
                { "ArmorSetFlag", (parser, x) => x.ArmorSetFlag = parser.ParseEnum<ArmorSetCondition>() },
                { "IgnoreArmorUpgrade", (parser, x) => x.IgnoreArmorUpgrade = parser.ParseBoolean() }
            });

        public ArmorSetCondition ArmorSetFlag { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IgnoreArmorUpgrade { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ArmorUpgrade(gameObject, this);
        }
    }
}

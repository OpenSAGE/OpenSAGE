using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the <see cref="SpawnBehaviorModuleData"/> module in order to work properly.
    /// </summary>
    public sealed class HiveStructureBodyModuleData : ActiveBodyModuleData
    {
        internal static new HiveStructureBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HiveStructureBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<HiveStructureBodyModuleData>
            {
                { "PropagateDamageTypesToSlavesWhenExisting", (parser, x) => x.PropagateDamageTypesToSlavesWhenExisting = parser.ParseEnumBitArray<DamageType>() },
                { "SwallowDamageTypesIfSlavesNotExisting", (parser, x) => x.SwallowDamageTypesIfSlavesNotExisting = parser.ParseEnumBitArray<DamageType>() }
            });

        public BitArray<DamageType> PropagateDamageTypesToSlavesWhenExisting { get; private set; }
        public BitArray<DamageType> SwallowDamageTypesIfSlavesNotExisting { get; private set; }
    }
}

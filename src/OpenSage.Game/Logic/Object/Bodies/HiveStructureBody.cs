using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the <see cref="SpawnBehavior"/> module in order to work properly.
    /// </summary>
    public sealed class HiveStructureBody : ObjectBody
    {
        internal static HiveStructureBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HiveStructureBody> FieldParseTable = new IniParseTable<HiveStructureBody>
        {
            { "PropagateDamageTypesToSlavesWhenExisting", (parser, x) => x.PropagateDamageTypesToSlavesWhenExisting = parser.ParseEnumBitArray<DamageType>() },
            { "SwallowDamageTypesIfSlavesNotExisting", (parser, x) => x.SwallowDamageTypesIfSlavesNotExisting = parser.ParseEnumBitArray<DamageType>() }
        }.Concat<HiveStructureBody, ObjectBody>(BodyFieldParseTable);

        public BitArray<DamageType> PropagateDamageTypesToSlavesWhenExisting { get; private set; }
        public BitArray<DamageType> SwallowDamageTypesIfSlavesNotExisting { get; private set; }
    }
}

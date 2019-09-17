using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ReflectDamageModuleData : DamageModuleData
    {
        internal static ReflectDamageModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ReflectDamageModuleData> FieldParseTable = new IniParseTable<ReflectDamageModuleData>
        {
            { "DamageTypesToReflect", (parser, x) => x.DamageTypesToReflect = parser.ParseEnumBitArray<DamageType>() },
            { "ReflectDamagePercentage", (parser, x) => x.ReflectDamagePercentage = parser.ParsePercentage() },
            { "MinimumDamageToReflect", (parser, x) => x.MinimumDamageToReflect = parser.ParseFloat() }
        };

        public BitArray<DamageType> DamageTypesToReflect { get; private set; }
        public Percentage ReflectDamagePercentage { get; private set; }
        public float MinimumDamageToReflect { get; private set; }
    }
}

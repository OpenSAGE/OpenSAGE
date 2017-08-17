using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the object specified in <see cref="HoleName"/> to have the REBUILD_HOLE KindOf and 
    /// <see cref="RebuildHoleBehavior"/> module in order to work.
    /// </summary>
    public sealed class RebuildHoleExposeDie : ObjectBehavior
    {
        internal static RebuildHoleExposeDie Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RebuildHoleExposeDie> FieldParseTable = new IniParseTable<RebuildHoleExposeDie>
        {
            { "HoleName", (parser, x) => x.HoleName = parser.ParseAssetReference() },
            { "HoleMaxHealth", (parser, x) => x.HoleMaxHealth = parser.ParseFloat() },
        };

        public string HoleName { get; private set; }
        public float HoleMaxHealth { get; private set; }
    }
}

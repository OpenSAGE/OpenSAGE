using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the object specified in <see cref="HoleName"/> to have the REBUILD_HOLE KindOf and 
    /// <see cref="RebuildHoleBehavior"/> module in order to work.
    /// </summary>
    public sealed class RebuildHoleExposeDieBehavior : ObjectBehavior
    {
        internal static RebuildHoleExposeDieBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RebuildHoleExposeDieBehavior> FieldParseTable = new IniParseTable<RebuildHoleExposeDieBehavior>
        {
            { "HoleName", (parser, x) => x.HoleName = parser.ParseAssetReference() },
            { "HoleMaxHealth", (parser, x) => x.HoleMaxHealth = parser.ParseFloat() },
        };

        public string HoleName { get; private set; }
        public float HoleMaxHealth { get; private set; }
    }
}

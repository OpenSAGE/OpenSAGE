using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class LocomotorSet
    {
        internal static LocomotorSet Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<LocomotorSet> FieldParseTable = new IniParseTable<LocomotorSet>
        {
            { "Locomotor", (parser, x) => x.Locomotor = parser.ParseLocomotorTemplateReference() },
            { "Condition", (parser, x) => x.Condition = parser.ParseEnum<LocomotorSetType>() },
            { "Speed", (parser, x) => x.Speed = parser.ParseFloat() },
        };

        public LazyAssetReference<LocomotorTemplate> Locomotor { get; internal set; }
        public LocomotorSetType Condition { get; internal set; }
        public float Speed { get; internal set; }
    }
}

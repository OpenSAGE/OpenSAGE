using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class LocomotorSetTemplate
    {
        internal static LocomotorSetTemplate Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<LocomotorSetTemplate> FieldParseTable = new IniParseTable<LocomotorSetTemplate>
        {
            { "Locomotor", (parser, x) => x.Locomotors = new[] { parser.ParseLocomotorTemplateReference() } },
            { "Condition", (parser, x) => x.Condition = parser.ParseEnum<LocomotorSetType>() },
            { "Speed", (parser, x) => x.Speed = parser.ParseFloat() },
        };

        public LazyAssetReference<LocomotorTemplate>[] Locomotors { get; internal set; }
        public LocomotorSetType Condition { get; internal set; }
        public float Speed { get; internal set; }
    }
}

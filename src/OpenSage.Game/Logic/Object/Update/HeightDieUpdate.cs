using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class HeightDieUpdate : ObjectBehavior
    {
        internal static HeightDieUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HeightDieUpdate> FieldParseTable = new IniParseTable<HeightDieUpdate>
        {
            { "TargetHeight", (parser, x) => x.TargetHeight = parser.ParseFloat() },
            { "TargetHeightIncludesStructures", (parser, x) => x.TargetHeightIncludesStructures = parser.ParseBoolean() },
            { "DestroyAttachedParticlesAtHeight", (parser, x) => x.DestroyAttachedParticlesAtHeight = parser.ParseFloat() },
        };

        public float TargetHeight { get; private set; }
        public bool TargetHeightIncludesStructures { get; private set; }

        /// <summary>
        /// INI comment indicates that this is a hack, and should be removed...
        /// </summary>
        public float DestroyAttachedParticlesAtHeight { get; private set; }
    }
}

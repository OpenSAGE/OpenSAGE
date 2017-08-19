using System;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// INI file comments indicate that this is not an accurate name; it's a really a 
    /// single mine behaviour.
    /// </summary>
    public sealed class MinefieldBehavior : ObjectBehavior
    {
        internal static MinefieldBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MinefieldBehavior> FieldParseTable = new IniParseTable<MinefieldBehavior>
        {
            { "DetonationWeapon", (parser, x) => x.DetonationWeapon = parser.ParseAssetReference() },
            { "DetonatedBy", (parser, x) => x.DetonatedBy = parser.ParseEnumFlags<MinefieldDetonatedBy>() },
            { "ScootFromStartingPointTime", (parser, x) => x.ScootFromStartingPointTime = parser.ParseInteger() },
            { "RepeatDetonateMoveThresh", (parser, x) => x.RepeatDetonateMoveThresh = parser.ParseFloat() },
            { "NumVirtualMines", (parser, x) => x.NumVirtualMines = parser.ParseInteger() },
            { "Regenerates", (parser, x) => x.Regenerates = parser.ParseBoolean() },
            { "StopsRegenAfterCreatorDies", (parser, x) => x.StopsRegenAfterCreatorDies = parser.ParseBoolean() },
            { "DegenPercentPerSecondAfterCreatorDies", (parser, x) => x.DegenPercentPerSecondAfterCreatorDies = parser.ParsePercentage() },
        };

        public string DetonationWeapon { get; private set; }
        public MinefieldDetonatedBy DetonatedBy { get; private set; }
        public int ScootFromStartingPointTime { get; private set; }
        public float RepeatDetonateMoveThresh { get; private set; }
        public int NumVirtualMines { get; private set; }
        public bool Regenerates { get; private set; }
        public bool StopsRegenAfterCreatorDies { get; private set; }
        public float DegenPercentPerSecondAfterCreatorDies { get; private set; }
    }

    [Flags]
    public enum MinefieldDetonatedBy
    {
        None = 0,

        [IniEnum("ENEMIES")]
        Enemies = 1 << 0,

        [IniEnum("NEUTRAL")]
        Neutral = 1 << 2,
    }
}

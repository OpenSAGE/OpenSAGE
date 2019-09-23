using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.LivingWorld
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldSound
    {
        internal static LivingWorldSound Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldSound> FieldParseTable = new IniParseTable<LivingWorldSound>
        {
            { "Position", (parser, x) => x.Position = parser.ParseVector3() },
            { "Sound", (parser, x) => x.Sound = parser.ParseAssetReference() },
            { "Flags", (parser, x) => x.Flags = parser.ParseEnum<LivingWorldSoundFlags>() },
            { "ZoomRegionLow", (parser, x) => x.ZoomRegionLow = parser.ParseVector2() },
            { "ZoomRegionHigh", (parser, x) => x.ZoomRegionHigh = parser.ParseVector2() },
        };

        public string Name { get; private set; }

        public Vector3 Position { get; private set; }
        public string Sound { get; private set; }
        public LivingWorldSoundFlags Flags { get; private set; }
        public Vector2 ZoomRegionLow  { get; private set; }
        public Vector2 ZoomRegionHigh  { get; private set; }
    }

    public enum LivingWorldSoundFlags
    {
        [IniEnum("VISIBLE")]
        Visible,

        [IniEnum("ZOOMING_IN")]
        ZoomingIn,
    }
}

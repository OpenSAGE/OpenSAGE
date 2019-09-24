using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class Multisound : BaseAsset
    {
        internal static Multisound Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("Multisound", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<Multisound> FieldParseTable = new IniParseTable<Multisound>
        {
            { "Subsounds", (parser, x) => x.Subsounds = parser.ParseAssetReferenceArray() },
            { "Control", (parser, x) => x.Control = parser.ParseEnum<MultisoundControl>() },
        };

        public string[] Subsounds { get; private set; }
        public MultisoundControl Control { get; private set; }
    }

    public enum MultisoundControl
    {
        [IniEnum("PLAY_ONE")]
        PlayOne,
    }
}

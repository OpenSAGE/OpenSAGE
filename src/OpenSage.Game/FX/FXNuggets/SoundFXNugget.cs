using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public sealed class SoundFXNugget : FXNugget
    {
        internal static SoundFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SoundFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<SoundFXNugget>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() }
        });

        public string Name { get; private set; }
    }
}

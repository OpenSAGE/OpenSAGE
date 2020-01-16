using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    public sealed class SoundFXNuggetData : FXNuggetData
    {
        internal static SoundFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SoundFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<SoundFXNuggetData>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() }
        });

        public string Name { get; private set; }
    }
}

using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AttachedModelFXNugget : FXNugget
    {
        internal static AttachedModelFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AttachedModelFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<AttachedModelFXNugget>
        {
            { "Modelname", (parser, x) => x.ModelName = parser.ParseAssetReference() }
        });

        public string ModelName { get; private set; }
    }
}

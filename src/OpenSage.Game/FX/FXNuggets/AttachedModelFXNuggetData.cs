using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AttachedModelFXNuggetData : FXNuggetData
    {
        internal static AttachedModelFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AttachedModelFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<AttachedModelFXNuggetData>
        {
            { "Modelname", (parser, x) => x.ModelName = parser.ParseAssetReference() }
        });

        public string ModelName { get; private set; }
    }
}

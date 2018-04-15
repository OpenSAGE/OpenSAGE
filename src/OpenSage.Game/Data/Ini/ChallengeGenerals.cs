using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class ChallengeGenerals
    {
        internal static ChallengeGenerals Parse(IniParser parser)
        {
            var result = new ChallengeGenerals();

            while (true)
            {
                parser.GoToNextLine();

                var token = parser.GetNextTokenOptional();
                if (token == null)
                {
                    continue;
                }
                else if (token.Value.Text.ToUpperInvariant() == IniParser.EndToken)
                {
                    break;
                }
                else
                {
                    var personaName = token.Value.Text;

                    result.Personas[personaName] = GeneralPersona.Parse(parser);
                }
            }

            return result;
        }

        public string Name { get; private set; }

        public Dictionary<string, GeneralPersona> Personas { get; } = new Dictionary<string, GeneralPersona>();
    }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class GeneralPersona
    {
        internal static GeneralPersona Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GeneralPersona> FieldParseTable = new IniParseTable<GeneralPersona>
        {
            { "PlayerTemplate", (parser, x) => x.PlayerTemplate = parser.ParseAssetReference() },
            { "StartsEnabled", (parser, x) => x.StartsEnabled = parser.ParseBoolean() },
            { "BioNameString", (parser, x) => x.BioNameString = parser.ParseLocalizedStringKey() },
            { "BioDOBString", (parser, x) => x.BioDateOfBirthString = parser.ParseLocalizedStringKey() },
            { "BioBirthplaceString", (parser, x) => x.BioBirthplaceString = parser.ParseLocalizedStringKey() },
            { "BioStrategyString", (parser, x) => x.BioStrategyString = parser.ParseLocalizedStringKey() },
            { "BioRankString", (parser, x) => x.BioRankString = parser.ParseLocalizedStringKey() },
            { "BioBranchString", (parser, x) => x.BioBranchString = parser.ParseLocalizedStringKey() },
            { "BioClassNumberString", (parser, x) => x.BioClassNumberString = parser.ParseLocalizedStringKey() },
            { "BioPortraitSmall", (parser, x) => x.BioPortraitSmall = parser.ParseAssetReference() },
            { "BioPortraitLarge", (parser, x) => x.BioPortraitLarge = parser.ParseAssetReference() },
            { "PortraitMovieLeftName", (parser, x) => x.PortraitMovieLeftName = parser.ParseAssetReference() },
            { "PortraitMovieRightName", (parser, x) => x.PortraitMovieRightName = parser.ParseAssetReference() },
            { "DefeatedImage", (parser, x) => x.DefeatedImage = parser.ParseAssetReference() },
            { "VictoriousImage", (parser, x) => x.VictoriousImage = parser.ParseAssetReference() },
            { "DefeatedString", (parser, x) => x.DefeatedString = parser.ParseAssetReference() },
            { "VictoriousString", (parser, x) => x.VictoriousString = parser.ParseAssetReference() },
            { "SelectionSound", (parser, x) => x.SelectionSound = parser.ParseAssetReference() },
            { "PreviewSound", (parser, x) => x.PreviewSound = parser.ParseAssetReference() },
            { "TauntSound1", (parser, x) => x.TauntSound1 = parser.ParseAssetReference() },
            { "TauntSound2", (parser, x) => x.TauntSound2 = parser.ParseAssetReference() },
            { "TauntSound3", (parser, x) => x.TauntSound3 = parser.ParseAssetReference() },
            { "WinSound", (parser, x) => x.WinSound = parser.ParseAssetReference() },
            { "LossSound", (parser, x) => x.LossSound = parser.ParseAssetReference() },
            { "NameSound", (parser, x) => x.NameSound = parser.ParseAssetReference() },
            { "Campaign", (parser, x) => x.Campaign = parser.ParseAssetReference() },
        };

        public string PlayerTemplate { get; private set; }
        public bool StartsEnabled { get; private set; }
        public string BioNameString { get; private set; }
        public string BioDateOfBirthString { get; private set; }
        public string BioBirthplaceString { get; private set; }
        public string BioStrategyString { get; private set; }
        public string BioRankString { get; private set; }
        public string BioBranchString { get; private set; }
        public string BioClassNumberString { get; private set; }
        public string BioPortraitSmall { get; private set; }
        public string BioPortraitLarge { get; private set; }
        public string PortraitMovieLeftName { get; private set; }
        public string PortraitMovieRightName { get; private set; }
        public string DefeatedImage { get; private set; }
        public string VictoriousImage { get; private set; }
        public string DefeatedString { get; private set; }
        public string VictoriousString { get; private set; }
        public string SelectionSound { get; private set; }
        public string PreviewSound { get; private set; }
        public string TauntSound1 { get; private set; }
        public string TauntSound2 { get; private set; }
        public string TauntSound3 { get; private set; }
        public string WinSound { get; private set; }
        public string LossSound { get; private set; }
        public string NameSound { get; private set; }
        public string Campaign { get; private set; }
    }
}

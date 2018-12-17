using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Shows/hides sub-objects on this object's model via upgrading.
    /// </summary>
    public sealed class SubObjectsUpgradeModuleData : UpgradeModuleData
    {
        internal static SubObjectsUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SubObjectsUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<SubObjectsUpgradeModuleData>
            {
                { "ShowSubObjects", (parser, x) => x.ShowSubObjects = parser.ParseAssetReferenceArray() },
                { "HideSubObjects", (parser, x) => x.HideSubObjects = parser.ParseAssetReferenceArray() },
                { "UpgradeTexture", (parser, x) => x.UpgradeTextures.Add(RandomTexture.Parse(parser)) },
                { "RecolorHouse", (parser, x) => x.RecolorHouse = parser.ParseBoolean() },
                { "ExcludeSubobjects", (parser, x) => x.ExcludeSubobjects = parser.ParseAssetReferenceArray() },
                { "SkipFadeOnCreate", (parser, x) => x.SkipFadeOnCreate = parser.ParseBoolean() },
                { "FadeTimeInSeconds", (parser, x) => x.FadeTimeInSeconds = parser.ParseFloat() },
                { "WaitBeforeFadeInSeconds", (parser, x) => x.WaitBeforeFadeInSeconds = parser.ParseFloat() },
                { "FXListUpgrade", (parser, x) => x.FXListUpgrade = parser.ParseAssetReference() },
                { "HideSubObjectsOnRemove", (parser, x) => x.HideSubObjectsOnRemove = parser.ParseBoolean() }
            });

        public string[] ShowSubObjects { get; private set; }
        public string[] HideSubObjects { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<RandomTexture> UpgradeTextures { get; private set; } = new List<RandomTexture>();

        [AddedIn(SageGame.Bfme)]
        public bool RecolorHouse { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] ExcludeSubobjects { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool SkipFadeOnCreate { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float FadeTimeInSeconds { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float WaitBeforeFadeInSeconds { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string FXListUpgrade { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool HideSubObjectsOnRemove { get; private set; }
    }
}

using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows an object to create/spawn a new object via upgrades.
    /// </summary>
    public sealed class ObjectCreationUpgradeModuleData : UpgradeModuleData
    {
        internal static ObjectCreationUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ObjectCreationUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<ObjectCreationUpgradeModuleData>
            {
                { "UpgradeObject", (parser, x) => x.UpgradeObject = parser.ParseAssetReference() },
                { "Delay", (parser, x) => x.Delay = parser.ParseFloat() },
                { "RemoveUpgrade", (parser, x) => x.RemoveUpgrade = parser.ParseAssetReference() },
                { "GrantUpgrade", (parser, x) => x.GrantUpgrade = parser.ParseAssetReference() },
                { "DestroyWhenSold", (parser, x) => x.DestroyWhenSold = parser.ParseBoolean() },
                { "DeathAnimAndDuration", (parser, x) => x.DeathAnimAndDuration = AnimAndDuration.Parse(parser) },
                { "Offset", (parser, x) => x.Offset = parser.ParseVector3() },
                { "ThingToSpawn", (parser, x) => x.ThingToSpawn = parser.ParseAssetReference() },
                { "FadeInTime", (parser, x) => x.FadeInTime = parser.ParseInteger() },
                { "UseBuildingProduction", (parser, x) => x.UseBuildingProduction = parser.ParseBoolean() }
            });

        public string UpgradeObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float Delay { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string RemoveUpgrade { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string GrantUpgrade { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool DestroyWhenSold { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public AnimAndDuration DeathAnimAndDuration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector3 Offset { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ThingToSpawn { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FadeInTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseBuildingProduction { get; private set; }
    }
}

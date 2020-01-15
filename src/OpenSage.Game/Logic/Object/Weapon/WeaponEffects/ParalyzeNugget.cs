using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class ParalyzeNugget : WeaponEffectNuggetData
    {
        internal static ParalyzeNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ParalyzeNugget> FieldParseTable = WeaponEffectNuggetData.FieldParseTable
            .Concat(new IniParseTable<ParalyzeNugget>
            {
                { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
                { "Duration", (parser, x) => x.Duration = parser.ParseInteger() },
                { "ParalyzeFX", (parser, x) => x.ParalyzeFX = parser.ParseAssetReference() },
                { "FreezeAnimation", (parser, x) => x.FreezeAnimation = parser.ParseAssetReference() },
                { "AffectHordeMembers", (parser, x) => x.AffectHordeMembers = parser.ParseBoolean() },
            });

        public float Radius { get; private set; }
        public int Duration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ParalyzeFX { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string FreezeAnimation { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool AffectHordeMembers { get; private set; }
    }
}

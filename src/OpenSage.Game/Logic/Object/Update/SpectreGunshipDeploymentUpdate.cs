using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SpectreGunshipDeploymentUpdate : ObjectBehavior
    {
        internal static SpectreGunshipDeploymentUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpectreGunshipDeploymentUpdate> FieldParseTable = new IniParseTable<SpectreGunshipDeploymentUpdate>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "RequiredScience", (parser, x) => x.RequiredScience = parser.ParseAssetReference() },
            { "GunshipTemplateName", (parser, x) => x.GunshipTemplateName = parser.ParseAssetReference() },
            { "AttackAreaRadius", (parser, x) => x.AttackAreaRadius = parser.ParseInteger() },
            { "CreateLocation", (parser, x) => x.CreateLocation = parser.ParseEnum<OCLCreationPoint>() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public string RequiredScience { get; private set; }
        public string GunshipTemplateName { get; private set; }
        public int AttackAreaRadius { get; private set; }
        public OCLCreationPoint CreateLocation { get; private set; }
    }
}

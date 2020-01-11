using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HordeAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new HordeAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HordeAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<HordeAIUpdateModuleData>
            {
                { "ComboLocomotorSet", (parser, x) => x.ComboLocomotorSet = parser.ParseAssetReference() },
                { "ComboLocoAttackDistance", (parser, x) => x.ComboLocoAttackDistance = parser.ParseInteger() },
            });

        public string ComboLocomotorSet { get; private set; }
        public int ComboLocoAttackDistance { get; private set; }
    }
}

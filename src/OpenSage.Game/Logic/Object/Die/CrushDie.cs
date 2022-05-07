using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CrushDie : DieModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    /// <summary>
    /// Allows for the use of the FRONTCRUSHED and BACKCRUSHED condition states.
    /// </summary>
    public sealed class CrushDieModuleData : DieModuleData
    {
        internal static CrushDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CrushDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<CrushDieModuleData>
            {
                { "TotalCrushSound", (parser, x) => x.TotalCrushSound = parser.ParseAssetReference() },
                { "BackEndCrushSound", (parser, x) => x.BackEndCrushSound = parser.ParseAssetReference() },
                { "FrontEndCrushSound", (parser, x) => x.FrontEndCrushSound = parser.ParseAssetReference() },
                { "TotalCrushSoundPercent", (parser, x) => x.TotalCrushSoundPercent = parser.ParseInteger() },
                { "BackEndCrushSoundPercent", (parser, x) => x.BackEndCrushSoundPercent = parser.ParseInteger() },
                { "FrontEndCrushSoundPercent", (parser, x) => x.FrontEndCrushSoundPercent = parser.ParseInteger() }
            });
        
        public string TotalCrushSound { get; private set; }
        public string BackEndCrushSound { get; private set; }
        public string FrontEndCrushSound { get; private set; }
        public int TotalCrushSoundPercent { get; private set; }
        public int BackEndCrushSoundPercent { get; private set; }
        public int FrontEndCrushSoundPercent { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CrushDie();
        }
    }
}

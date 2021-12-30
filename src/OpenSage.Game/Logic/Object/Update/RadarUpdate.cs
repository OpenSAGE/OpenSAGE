using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class RadarUpdate : UpdateModule
    {
        private uint _radarExtendEndFrame;
        private bool _isRadarExtending;
        private bool _isRadarExtended;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistUInt32(ref _radarExtendEndFrame);
            reader.PersistBoolean(ref _isRadarExtended);
            reader.PersistBoolean(ref _isRadarExtending);
        }
    }

    public sealed class RadarUpdateModuleData : UpdateModuleData
    {
        internal static RadarUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RadarUpdateModuleData> FieldParseTable = new IniParseTable<RadarUpdateModuleData>
        {
            { "RadarExtendTime", (parser, x) => x.RadarExtendTime = parser.ParseInteger() }
        };
        
        public int RadarExtendTime { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new RadarUpdate();
        }
    }
}

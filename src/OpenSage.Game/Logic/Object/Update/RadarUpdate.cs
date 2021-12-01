using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class RadarUpdate : UpdateModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            for (var i = 0; i < 6; i++)
            {
                var unknown = reader.ReadByte();
                if (unknown != 0)
                {
                    throw new InvalidStateException();
                }
            }
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

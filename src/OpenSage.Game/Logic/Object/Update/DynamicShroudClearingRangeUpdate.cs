using OpenSage.Data.Ini;
using OpenSage.Gui.InGame;

namespace OpenSage.Logic.Object
{
    public sealed class DynamicShroudClearingRangeUpdate : UpdateModule
    {
        private uint _unknown1;
        private uint _unknown2;
        private uint _unknown3;
        private uint _unknown4;
        private uint _unknown5;
        private uint _unknown6;
        private uint _unknown7;
        private float _unknownFloat;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistUInt32(ref _unknown1);
            reader.PersistUInt32(ref _unknown2);
            reader.PersistUInt32(ref _unknown3);
            reader.PersistUInt32(ref _unknown4);
            reader.PersistUInt32(ref _unknown5);
            reader.PersistUInt32(ref _unknown6);
            reader.PersistUInt32(ref _unknown7);

            var unknown7_1 = true;
            reader.PersistBoolean("Unknown7_1", ref unknown7_1);
            if (!unknown7_1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(4);

            var unknown9 = 300.0f;
            reader.PersistSingle(ref unknown9);
            if (unknown9 != 300.0f)
            {
                throw new InvalidStateException();
            }

            reader.PersistSingle(ref _unknownFloat);
        }
    }

    public sealed class DynamicShroudClearingRangeUpdateModuleData : UpdateModuleData
    {
        internal static DynamicShroudClearingRangeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DynamicShroudClearingRangeUpdateModuleData> FieldParseTable = new IniParseTable<DynamicShroudClearingRangeUpdateModuleData>
        {
            { "FinalVision", (parser, x) => x.FinalVision = parser.ParseFloat() },
            { "ChangeInterval", (parser, x) => x.ChangeInterval = parser.ParseInteger() },
            { "ShrinkDelay", (parser, x) => x.ShrinkDelay = parser.ParseInteger() },
            { "ShrinkTime", (parser, x) => x.ShrinkTime = parser.ParseInteger() },
            { "GrowDelay", (parser, x) => x.GrowDelay = parser.ParseInteger() },
            { "GrowTime", (parser, x) => x.GrowTime = parser.ParseInteger() },
            { "GrowInterval", (parser, x) => x.GrowInterval = parser.ParseInteger() },

            { "GridDecalTemplate", (parser, x) => x.GridDecalTemplate = RadiusDecalTemplate.Parse(parser) }
        };

        public float FinalVision { get; private set; }
        public int ChangeInterval { get; private set; }
        public int ShrinkDelay { get; private set; }
        public int ShrinkTime { get; private set; }
        public int GrowDelay { get; private set; }
        public int GrowTime { get; private set; }
        public int GrowInterval { get; private set; }

        public RadiusDecalTemplate GridDecalTemplate { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DynamicShroudClearingRangeUpdate();
        }
    }
}

using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class MinefieldBehavior : UpdateModule
    {
        private uint _numVirtualMachines;
        private uint _unknownFrame;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistUInt32("NumVirtualMachines", ref _numVirtualMachines);
            reader.PersistFrame("UnknownFrame", ref _unknownFrame);

            reader.SkipUnknownBytes(29);

            ushort unknown2 = 1;
            reader.PersistUInt16(ref unknown2);
            if (unknown2 != 1)
            {
                throw new InvalidStateException();
            }

            ushort unknown3 = 3;
            reader.PersistUInt16(ref unknown3);
            if (unknown3 != 3)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(23);
        }
    }

    /// <summary>
    /// INI file comments indicate that this is not an accurate name; it's a really a 
    /// single mine behaviour.
    /// </summary>
    public sealed class MinefieldBehaviorModuleData : BehaviorModuleData
    {
        internal static MinefieldBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<MinefieldBehaviorModuleData> FieldParseTable = new IniParseTable<MinefieldBehaviorModuleData>
        {
            { "DetonationWeapon", (parser, x) => x.DetonationWeapon = parser.ParseAssetReference() },
            { "DetonatedBy", (parser, x) => x.DetonatedBy = parser.ParseEnumBitArray<ObjectFilterRelationship>() },
            { "ScootFromStartingPointTime", (parser, x) => x.ScootFromStartingPointTime = parser.ParseInteger() },
            { "RepeatDetonateMoveThresh", (parser, x) => x.RepeatDetonateMoveThresh = parser.ParseFloat() },
            { "NumVirtualMines", (parser, x) => x.NumVirtualMines = parser.ParseInteger() },
            { "Regenerates", (parser, x) => x.Regenerates = parser.ParseBoolean() },
            { "StopsRegenAfterCreatorDies", (parser, x) => x.StopsRegenAfterCreatorDies = parser.ParseBoolean() },
            { "DegenPercentPerSecondAfterCreatorDies", (parser, x) => x.DegenPercentPerSecondAfterCreatorDies = parser.ParsePercentage() },
        };

        public string DetonationWeapon { get; private set; }
        public BitArray<ObjectFilterRelationship> DetonatedBy { get; private set; }
        public int ScootFromStartingPointTime { get; private set; }
        public float RepeatDetonateMoveThresh { get; private set; }
        public int NumVirtualMines { get; private set; }
        public bool Regenerates { get; private set; }
        public bool StopsRegenAfterCreatorDies { get; private set; }
        public Percentage DegenPercentPerSecondAfterCreatorDies { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new MinefieldBehavior();
        }
    }

    public enum ObjectFilterRelationship
    {
        [IniEnum("ENEMIES")]
        Enemies,

        [IniEnum("NEUTRAL")]
        Neutral,
    }
}

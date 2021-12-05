using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class DozerAIUpdate : AIUpdate
    {
        internal DozerAIUpdate(GameObject gameObject, DozerAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            // Following is same as WorkerAIUpdate.Load

            var unknown1 = reader.ReadInt32();
            if (unknown1 != 3)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < 3; i++)
            {
                var objectId = reader.ReadObjectID();

                var unknown = reader.ReadInt32();
            }

            var stateMachine = new WorkerAIUpdateStateMachine1();
            stateMachine.Load(reader);

            var unknown2 = reader.ReadInt32();

            var unknown3 = reader.ReadInt32();
            if (unknown3 != 3)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < 9; i++)
            {
                var unknown4 = reader.ReadBoolean();
                var unknownPos = reader.ReadVector3();
            }

            var unknown5 = reader.ReadInt32();
        }
    }

    /// <summary>
    /// Allows the use of VoiceRepair, VoiceBuildResponse, VoiceNoBuild and VoiceTaskComplete 
    /// within UnitSpecificSounds section of the object.
    /// Requires Kindof = DOZER.
    /// </summary>
    public sealed class DozerAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static DozerAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<DozerAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<DozerAIUpdateModuleData>
            {
                { "RepairHealthPercentPerSecond", (parser, x) => x.RepairHealthPercentPerSecond = parser.ParsePercentage() },
                { "BoredTime", (parser, x) => x.BoredTime = parser.ParseInteger() },
                { "BoredRange", (parser, x) => x.BoredRange = parser.ParseInteger() },
            });

        public Percentage RepairHealthPercentPerSecond { get; private set; }
        public int BoredTime { get; private set; }
        public int BoredRange { get; private set; }

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new DozerAIUpdate(gameObject, this);
        }
    }
}

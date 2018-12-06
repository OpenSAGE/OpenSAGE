using System.Numerics;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of StartDive within UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class DeliverPayloadAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new DeliverPayloadAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DeliverPayloadAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<DeliverPayloadAIUpdateModuleData>
            {
                { "DoorDelay", (parser, x) => x.DoorDelay = parser.ParseInteger() },
                { "MaxAttempts", (parser, x) => x.MaxAttempts = parser.ParseInteger() },
                { "DropOffset", (parser, x) => x.DropOffset = parser.ParseVector3() },
                { "DropDelay", (parser, x) => x.DropDelay = parser.ParseInteger() },
                { "PutInContainer", (parser, x) => x.PutInContainer = parser.ParseAssetReference() },
                { "DeliveryDistance", (parser, x) => x.DeliveryDistance = parser.ParseInteger() }
            });

        public int DoorDelay { get; private set; }
        public int MaxAttempts { get; private set; }
        public Vector3 DropOffset { get; private set; }
        public int DropDelay { get; private set; }
        public string PutInContainer { get; private set; }
        public int DeliveryDistance { get; private set; }
    }
}

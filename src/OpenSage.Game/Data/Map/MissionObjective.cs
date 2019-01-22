using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Ra3)]
    public sealed class MissionObjective
    {
        public string ID { get; private set; }
        public string Text { get; private set; }
        public string Description { get; private set; }
        public bool IsBonusObjective { get; private set; }
        public MissionObjectiveType ObjectiveType { get; private set; }

        internal static MissionObjective Parse(BinaryReader reader)
        {
            return new MissionObjective
            {
                ID = reader.ReadUInt16PrefixedAsciiString(),
                Text = reader.ReadUInt16PrefixedAsciiString(),
                Description = reader.ReadUInt16PrefixedAsciiString(),
                IsBonusObjective = reader.ReadBooleanChecked(),
                ObjectiveType = reader.ReadUInt32AsEnum<MissionObjectiveType>()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteUInt16PrefixedAsciiString(ID);
            writer.WriteUInt16PrefixedAsciiString(Text);
            writer.WriteUInt16PrefixedAsciiString(Description);
            writer.Write(IsBonusObjective);
            writer.Write((uint) ObjectiveType);
        }
    }

    [AddedIn(SageGame.Ra3)]
    public enum MissionObjectiveType : uint
    {
        Attack = 0,
        Unknown1 = 1,
        Unknown2 = 2,
        Build = 3,
        Capture = 4,
        Move = 5,
        Protect = 6
    }
}

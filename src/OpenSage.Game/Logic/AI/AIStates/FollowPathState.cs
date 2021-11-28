using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FollowPathState : MoveTowardsState
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknown1 = reader.ReadUInt32();
            var unknown2 = reader.ReadBoolean();
            var unknown3 = reader.ReadBoolean();
        }
    }
}

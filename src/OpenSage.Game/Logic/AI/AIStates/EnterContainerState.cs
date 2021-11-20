using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class EnterContainerState : MoveTowardsState
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var containerObjectId = reader.ReadObjectID();
        }
    }
}

using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class ExitContainerState : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var containerObjectId = reader.ReadObjectID();
        }
    }
}

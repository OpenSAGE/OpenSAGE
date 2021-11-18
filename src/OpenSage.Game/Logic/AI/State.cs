using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI
{
    internal abstract class State
    {
        internal abstract void Load(SaveFileReader reader);
    }
}

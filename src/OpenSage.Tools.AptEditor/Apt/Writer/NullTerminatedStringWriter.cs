using System.Text;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static partial class DataWriter
    {
        public static uint WriteImpl(MemoryPool memory, string data)
        {
            memory.AllocateBytesForPadding(Constants.IntPtrSize);
            return memory.WriteDataToNewChunk(Encoding.ASCII.GetBytes(data + '\0')).StartAddress;
        }
    }
}
using System.IO;

namespace OpenSage.Data.Apt.FrameItems
{
    public sealed class RemoveObject : FrameItem
    {
        public int Depth { get; private set; }

        public static RemoveObject Parse(BinaryReader reader)
        {
            return new RemoveObject
            {
                Depth = reader.ReadInt32()
            };
        }

        public static RemoveObject Create(int depth)
        {
            return new RemoveObject
            {
                Depth = depth
            };
        }
    }
}

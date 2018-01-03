using System.IO;

namespace OpenSage.Data.Apt.FrameItems
{
    public sealed class RemoveObject : FrameItem
    {
        public int Depth { get; private set; }

        public static RemoveObject Parse(BinaryReader reader)
        {
            var removeObject = new RemoveObject();
            removeObject.Depth = reader.ReadInt32();
            return removeObject;
        }
    }
}

using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt.FrameItems
{
    public enum FrameItemType
    {
        Action = 1,
        FrameLabel = 2,
        PlaceObject = 3,
        RemoveObject = 4,
        BackgroundColor = 5,
        InitAction = 8
    };

    public abstract class FrameItem : IDataStorage
    {
        public static FrameItem Parse(BinaryReader reader)
        {
            FrameItem frameItem = null;

            var type = reader.ReadUInt32AsEnum<FrameItemType>();

            switch (type)
            {
                case FrameItemType.Action:
                    frameItem = Action.Parse(reader);
                    break;
                case FrameItemType.FrameLabel:
                    frameItem = FrameLabel.Parse(reader);
                    break;
                case FrameItemType.PlaceObject:
                    frameItem = PlaceObject.Parse(reader);
                    break;
                case FrameItemType.RemoveObject:
                    frameItem = RemoveObject.Parse(reader);
                    break;
                case FrameItemType.BackgroundColor:
                    frameItem = BackgroundColor.Parse(reader);
                    break;
                case FrameItemType.InitAction:
                    frameItem = InitAction.Parse(reader);
                    break;
            }

            return frameItem;
        }

        public abstract void Write(BinaryWriter writer, MemoryPool pool);
    }
}

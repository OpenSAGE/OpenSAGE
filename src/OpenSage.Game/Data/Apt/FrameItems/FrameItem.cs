using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.FrameItems
{
    enum FrameItemType
    {
        Action = 1,
        FrameLabel = 2,
        PlaceObject = 3,
        RemoveObject = 4,
        BackgroundColor = 5,
        InitAction = 8
    };

    public class FrameItem
    {
        public static FrameItem Create(BinaryReader reader)
        {
            FrameItem frameItem = null;

            var type = reader.ReadUInt32AsEnum<FrameItemType>();

            switch (type)
            {
                case FrameItemType.Action:
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
                    break;
            }

            return frameItem;
        }
    }
}

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
            FrameItem frameitem = null;

            var type = reader.ReadUInt32AsEnum<FrameItemType>();

            switch (type)
            {
                case FrameItemType.Action:
                    break;
                case FrameItemType.FrameLabel:
                    break;
                case FrameItemType.PlaceObject:
                    break;
                case FrameItemType.RemoveObject:
                    break;
                case FrameItemType.BackgroundColor:
                    break;
                case FrameItemType.InitAction:
                    break;
            }

            return frameitem;
        }
    }
}

using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.FrameItems
{
    enum FrameItemType
    {
        ACTION = 1,
        FRAMELABEL = 2,
        PLACEOBJECT = 3,
        REMOVEOBJECT = 4,
        BACKGROUNDCOLOR = 5,
        INITACTION = 8
    };

    public class FrameItem
    {
        public static FrameItem Create(BinaryReader br)
        {
            FrameItem fi = null;

            var type = br.ReadUInt32AsEnum<FrameItemType>();

            switch (type)
            {
                case FrameItemType.ACTION:
                    break;
                case FrameItemType.FRAMELABEL:
                    break;
                case FrameItemType.PLACEOBJECT:
                    break;
                case FrameItemType.REMOVEOBJECT:
                    break;
                case FrameItemType.BACKGROUNDCOLOR:
                    break;
                case FrameItemType.INITACTION:
                    break;
            }

            return fi;
        }
    }
}

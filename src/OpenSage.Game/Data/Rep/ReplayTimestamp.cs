using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Rep
{
    public sealed class ReplayTimestamp
    {
        public ushort Year { get; private set; }
        public ushort Month { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }
        public ushort Day { get; private set; }

        public ushort Hour { get; private set; }
        public ushort Minute { get; private set; }
        public ushort Second { get; private set; }
        public ushort Millisecond { get; private set; }

        internal static ReplayTimestamp Parse(BinaryReader reader)
        {
            return new ReplayTimestamp
            {
                Year = reader.ReadUInt16(),
                Month = reader.ReadUInt16(),
                DayOfWeek = reader.ReadUInt16AsEnum<DayOfWeek>(),
                Day = reader.ReadUInt16(),

                Hour = reader.ReadUInt16(),
                Minute = reader.ReadUInt16(),
                Second = reader.ReadUInt16(),
                Millisecond = reader.ReadUInt16()
            };
        }
    }
}

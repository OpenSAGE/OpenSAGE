using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Csf
{
    public sealed class CsfHeader
    {
        public CsfVersion Version { get; private set; }
        public uint LabelCount { get; private set; }
        public CsfLanguage Language { get; private set; }

        internal static CsfHeader Parse(BinaryReader reader)
        {
            var version = reader.ReadUInt32AsEnum<CsfVersion>();

            var labelCount = reader.ReadUInt32();

            var labelCount2 = reader.ReadUInt32();
            if (labelCount != labelCount2 + 1)
            {
                throw new InvalidDataException();
            }

            var magicValue = reader.ReadUInt32();
            if (magicValue != 0)
            {
                throw new InvalidDataException();
            }

            var language = reader.ReadUInt32AsEnum<CsfLanguage>();

            return new CsfHeader
            {
                Version = version,
                LabelCount = labelCount,
                Language = language
            };
        }
    }
}

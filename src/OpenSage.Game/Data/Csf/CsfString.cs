using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Csf
{
    public sealed class CsfString
    {
        public string Value { get; private set; }
        public string ExtraValue { get; private set; }

        internal static CsfString Parse(BinaryReader reader)
        {
            var fourCc = reader.ReadUInt32().ToFourCcString();
            if (fourCc != " RTS" && fourCc != "WRTS")
            {
                throw new InvalidDataException();
            }

            var value = reader.ReadUInt32PrefixedNegatedUnicodeString();

            string extraValue = null;
            if (fourCc == "WRTS")
            {
                extraValue = reader.ReadUInt32PrefixedAsciiString();
            }

            return new CsfString
            {
                Value = value,
                ExtraValue = extraValue
            };
        }
    }
}

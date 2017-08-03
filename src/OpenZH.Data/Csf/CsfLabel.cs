using System;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Csf
{
    public sealed class CsfLabel
    {
        public string Name { get; private set; }
        public CsfString[] Strings { get; private set; }

        internal static CsfLabel Parse(BinaryReader reader)
        {
            var fourCc = reader.ReadUInt32().ToFourCcString();
            if (fourCc != " LBL")
            {
                throw new InvalidDataException();
            }

            var stringCount = reader.ReadUInt32();
            if (stringCount > 1)
            {
                throw new NotSupportedException();
            }

            var name = reader.ReadUInt32PrefixedAsciiString();

            var strings = new CsfString[stringCount];
            for (var i = 0; i < stringCount; i++)
            {
                strings[i] = CsfString.Parse(reader);
            }

            return new CsfLabel
            {
                Name = name,
                Strings = strings
            };
        }
    }
}

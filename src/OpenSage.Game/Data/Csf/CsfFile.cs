﻿using System.IO;
using System.Text;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;
using OpenSage.IO;

namespace OpenSage.Data.Csf
{
    public sealed class CsfFile
    {
        public CsfHeader Header { get; private set; }
        public CsfLabel[] Labels { get; private set; }

        public static CsfFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var fourCc = reader.ReadFourCc(bigEndian: true);
                if (fourCc != "CSF ")
                {
                    throw new InvalidDataException();
                }

                var header = CsfHeader.Parse(reader);

                var labels = new CsfLabel[header.LabelCount];
                for (var i = 0; i < header.LabelCount; i++)
                {
                    labels[i] = CsfLabel.Parse(reader);
                }

                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException();
                }

                return new CsfFile
                {
                    Header = header,
                    Labels = labels
                };
            }
        }
    }
}

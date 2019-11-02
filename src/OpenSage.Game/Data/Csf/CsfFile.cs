﻿using System.IO;
using System.Text;
using OpenSage.Data.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Csf
{
    public sealed class CsfFile
    {
        public CsfHeader Header { get; private set; }
        public CsfLabel[] Labels { get; private set; }

        public static CsfFile FromUrl(string url)
        {
            using (var stream = FileSystem.OpenStream(url, IO.FileMode.Open))
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

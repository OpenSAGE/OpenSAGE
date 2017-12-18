using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public class AptFile
    {
        public ConstantData Constants { get; private set; }


        public AptFile(ConstantData c)
        {
            Constants = c;
        }

        public void Parse(BinaryReader br)
        {
            //jump to the entry offset
            var entryOffset = Constants.AptDataEntryOffset;
            br.BaseStream.Seek(entryOffset, SeekOrigin.Begin);

            //proceed loading the characters
            var movie = Character.Create(br);
        }

        public static AptFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                //check if this is a valid apt file
                var magic = reader.ReadFixedLengthString(8);
                if (magic != "Apt Data")
                {
                    throw new InvalidDataException();
                }

                //load the corresponding const entry
                var constPath = Path.ChangeExtension(entry.FilePath, ".const");
                var constFile = ConstantData.FromFileSystemEntry(entry.FileSystem.GetFile(constPath));

                AptFile apt = new AptFile(constFile);
                apt.Parse(reader);

                return apt;
            }
        }
    }
}

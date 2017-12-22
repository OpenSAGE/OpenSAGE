using System.IO;
using System.Text;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public class AptFile
    {
        public ConstantData Constants { get; private set; }
        public Movie Movie { get; private set; }
        internal bool IsEmpty = true;

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
            Movie = (Movie)Character.Create(br,this);

            //resolve imports
            foreach(var import in Movie.Imports)
            {

            }
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

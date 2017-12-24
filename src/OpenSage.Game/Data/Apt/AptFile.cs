using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public sealed class AptFile
    {
        public ConstantData Constants { get; private set; }
        public Movie Movie { get; private set; }
        internal bool IsEmpty = true;
        internal string MovieName;
        internal FileSystem FileSystem;

        private AptFile(ConstantData constants, FileSystem filesystem, string name)
        {
            Constants = constants;
            FileSystem = filesystem;
            MovieName = name;
        }

        private void Parse(BinaryReader reader)
        {
            //jump to the entry offset
            var entryOffset = Constants.AptDataEntryOffset;
            reader.BaseStream.Seek(entryOffset, SeekOrigin.Begin);

            //proceed loading the characters
            Movie = (Movie) Character.Create(reader, this);

            //set first character to itself
            Movie.Characters[0] = Movie;

            //resolve geometries
            foreach (Shape shape in Movie.Characters.FindAll((x)=>x is Shape))
            {
                var ruPath = MovieName + "_geometry/" + shape.Geometry + ".ru";
                var shapeEntry = FileSystem.GetFile(ruPath);
                var shapeGeometry = Geometry.FromFileSystemEntry(shapeEntry);
            }


            var importDict = new Dictionary<string, AptFile>();

            //resolve imports
            foreach (var import in Movie.Imports)
            {
                //open the apt file where our character is located
                AptFile importApt;

                if (importDict.ContainsKey(import.Movie))
                {
                    importApt = importDict[import.Movie];
                }
                else
                {
                    var importEntry = FileSystem.GetFile(Path.ChangeExtension(import.Movie, ".apt"));
                    importApt = AptFile.FromFileSystemEntry(importEntry);
                    importDict[import.Movie] = importApt;
                }

                //get the export from that apt and proceed
                var export = importApt.Movie.Exports.Find(x => x.Name == import.Name);

                //place the exported character inside our movie
                Movie.Characters[(int) import.Character] = importApt.Movie.Characters[(int) export.Character];
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

                var aptName = Path.GetFileNameWithoutExtension(entry.FilePath);

                var apt = new AptFile(constFile, entry.FileSystem, aptName);
                apt.Parse(reader);

                return apt;
            }
        }
    }
}

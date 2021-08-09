using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats;
using OpenSage.FileFormats.Apt.FrameItems;

namespace OpenSage.FileFormats.Apt
{
    public sealed class AptFile
    {
        public string ParentDirectory { get; }
        public ConstantData Constants { get; }
        public string MovieName { get; }

        public Movie Movie { get; private set; }
        public ImageMap ImageMap { get; private set; }
        public Dictionary<uint, Geometry> GeometryMap { get; private set; }
        public Dictionary<string, string> ImportMap { get; private set; }
        public Func<string, Stream> StreamGetter { get; private set; }

        internal bool IsEmpty = true;

        private AptFile(ConstantData constants, string filesystem, string name, Func<string, Stream> getter)
        {
            Constants = constants;
            ParentDirectory = filesystem;
            MovieName = name;
            StreamGetter = getter;
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

            //load the corresponding image map
            var datPath = Path.Combine(ParentDirectory, MovieName + ".dat");
            using (var datEntry = StreamGetter(datPath))
                ImageMap = ImageMap.FromFileSystemEntry(datEntry);

            //resolve geometries
            GeometryMap = new Dictionary<uint, Geometry>();
            foreach (Shape shape in Movie.Characters.FindAll((x) => x is Shape))
            {
                var ruPath = Path.Combine(ParentDirectory, MovieName + "_geometry", +shape.GeometryId + ".ru");
                using (var shapeEntry = StreamGetter(ruPath))
                {
                    var shapeGeometry = Geometry.FromFileSystemEntry(this, shapeEntry);
                    GeometryMap[shape.GeometryId] = shapeGeometry;
                }
            }

            ImportMap = new Dictionary<string, string>();
            //resolve imports
            foreach (var import in Movie.Imports)
            if (!ImportMap.ContainsKey(import.Movie))
                {
                    var importPath = Path.Combine(ParentDirectory, Path.ChangeExtension(import.Movie, ".apt"));
                    var importEntry = StreamGetter(importPath);
                    if(importEntry == null)
                        throw new FileNotFoundException("Cannot find imported file", importPath);
                    ImportMap[import.Movie] = importPath;

                    // Some dirty tricks to avoid the above exception
                    FromFileSystemEntry(importPath, StreamGetter);
                }                               

        }
        public static AptFile FromFileSystemEntry(string entryPath, Func<string, Stream> streamGetter)
        {
            var movieName = Path.GetFileNameWithoutExtension(entryPath);

            using (var reader = new BinaryReader(streamGetter(entryPath), Encoding.ASCII, true))
            {
                //check if this is a valid apt file
                var magic = reader.ReadFixedLengthString(8);
                if (magic != "Apt Data")
                {
                    throw new InvalidDataException();
                }

                //load the corresponding const entry
                var constPath = Path.ChangeExtension(entryPath, ".const");
                using (var stream = streamGetter(constPath))
                {
                    var constFile = ConstantData.FromFileSystemEntry(stream); 

                    // Path.Combine(entryPath, Path.ChangeExtension(import.Movie, ".apt"));
                    // FileSystem.GetFile()?

                    var apt = new AptFile(constFile, Path.GetDirectoryName(entryPath), movieName, streamGetter);
                    apt.Parse(reader);

                    return apt;
                }
            }
        }

        public static AptFile CreateEmpty(string name, int width, int height, int millisecondsPerFrame)
        {
            var constData = new ConstantData();
            var apt = new AptFile(constData, null, name, (_) => null)
            {
                ImageMap = new ImageMap(),
                GeometryMap = new Dictionary<uint, Geometry>(),
                ImportMap = new Dictionary<string, string>(), 
            };
            apt.Movie = Movie.CreateEmpty(apt, width, height, millisecondsPerFrame);
            return apt;
        }
    }
}

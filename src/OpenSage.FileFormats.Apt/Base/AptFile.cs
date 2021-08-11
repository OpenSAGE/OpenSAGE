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

        public void Parse(BinaryReader reader)
        {
            //jump to the entry offset
            var entryOffset = Constants.AptDataEntryOffset;
            reader.BaseStream.Seek(entryOffset, SeekOrigin.Begin);

            //proceed loading the characters
            Movie = (Movie) Character.Create(reader, this);

            //set first character to itself
            Movie.Characters[0] = Movie;
        }


        public static AptFile FromPath(string entryPath, Func<string, Stream> streamGetter)
        {
            var movieName = Path.GetFileNameWithoutExtension(entryPath);
            var parentDirectory = Path.GetDirectoryName(entryPath);

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

                    var apt = new AptFile(constFile, parentDirectory, movieName, streamGetter);
                    apt.Parse(reader);

                    //load the corresponding image map
                    var datPath = Path.Combine(parentDirectory, movieName + ".dat");
                    using (var datEntry = streamGetter(datPath))
                        apt.ImageMap = ImageMap.FromFileSystemEntry(datEntry);

                    //resolve geometries
                    apt.GeometryMap = new Dictionary<uint, Geometry>();
                    foreach (Shape shape in apt.Movie.Characters.FindAll((x) => x is Shape))
                    {
                        var ruPath = Path.Combine(parentDirectory, movieName + "_geometry", +shape.GeometryId + ".ru");
                        using (var shapeEntry = streamGetter(ruPath))
                        {
                            var shapeGeometry = Geometry.FromFileSystemEntry(apt, shapeEntry);
                            apt.GeometryMap[shape.GeometryId] = shapeGeometry;
                        }
                    }

                    var importMap = new Dictionary<string, string>();
                    //resolve imports
                    foreach (var import in apt.Movie.Imports)
                        if (!importMap.ContainsKey(import.Movie))
                        {
                            var importPath = Path.Combine(parentDirectory, Path.ChangeExtension(import.Movie, ".apt"));
                            var importEntry = streamGetter(importPath);
                            if (importEntry == null)
                                throw new FileNotFoundException("Cannot find imported file", importPath);
                            importMap[import.Movie] = importPath;

                            // Some dirty tricks to avoid the above exception
                            FromPath(importPath, streamGetter);
                        }
                    apt.ImportMap = importMap;

                    return apt;
                }
            }
        }

        public void WriteTo(string path, Func<string, Stream> streamGetter)
        {

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

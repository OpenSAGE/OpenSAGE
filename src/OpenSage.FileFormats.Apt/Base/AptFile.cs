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
        public string RootDirectory { get; }
        public ConstantData Constants { get; }
        public string MovieName { get; }

        public Movie Movie { get; private set; }
        public ImageMap ImageMap { get; private set; }
        public Dictionary<uint, Geometry> GeometryMap { get; private set; }
        public Dictionary<string, string> ImportMap { get; private set; }
        public Func<string, Stream> StreamGetter { get; private set; }

        internal bool IsEmpty = true;

        private AptFile(ConstantData constants, string rootDirectory, string name, Func<string, Stream> getter)
        {
            Constants = constants;
            RootDirectory = rootDirectory;
            MovieName = name;
            StreamGetter = getter;
        }

        public void CheckImportTree()
        {
            foreach (var import in Movie.Imports)
            {
                var importPath = Path.Combine(RootDirectory, Path.ChangeExtension(import.Movie, ".apt"));
                var importEntry = StreamGetter(importPath);
                if (importEntry == null)
                    throw new FileNotFoundException("Cannot find imported file", importPath);

                // Some dirty tricks to avoid the above exception
                var f = FromPath(importPath, StreamGetter);
                f.CheckImportTree();
            }
        }


        public static AptFile FromPath(string aptPath, Func<string, Stream> streamGetter)
        {
            var aptName = Path.GetFileNameWithoutExtension(aptPath);
            var rootDirectory = Path.GetDirectoryName(aptPath);

            var streamInput = streamGetter(aptPath);
            if (streamInput == null)
                throw new FileNotFoundException("Cannot find file", aptPath);
            using (var reader = new BinaryReader(streamInput, Encoding.ASCII, true))
            {
                //check if this is a valid apt file
                var magic = reader.ReadFixedLengthString(8);
                if (magic != "Apt Data")
                {
                    throw new InvalidDataException();
                }

                //load the corresponding const entry
                var constPath = Path.ChangeExtension(aptPath, ".const");
                using (var stream = streamGetter(constPath))
                {
                    var constFile = ConstantData.FromFileSystemEntry(stream); 

                    // Path.Combine(entryPath, Path.ChangeExtension(import.Movie, ".apt"));
                    // FileSystem.GetFile()?

                    // create container & load .apt file
                    var apt = new AptFile(constFile, rootDirectory, aptName, streamGetter);
                    var entryOffset = constFile.AptDataEntryOffset;
                    reader.BaseStream.Seek(entryOffset, SeekOrigin.Begin);

                    //proceed loading the characters
                    apt.Movie = (Movie) Character.Create(reader, apt);
                    //set first character to itself
                    apt.Movie.Characters[0] = apt.Movie;

                    //load the corresponding image map
                    var datPath = Path.Combine(rootDirectory, aptName + ".dat");
                    using (var datEntry = streamGetter(datPath))
                        apt.ImageMap = ImageMap.FromFileSystemEntry(datEntry);

                    //resolve geometries
                    apt.GeometryMap = new Dictionary<uint, Geometry>();
                    foreach (Shape shape in apt.Movie.Characters.FindAll((x) => x is Shape))
                    {
                        var ruPath = Path.Combine(rootDirectory, aptName + "_geometry", +shape.GeometryId + ".ru");
                        using (var shapeEntry = streamGetter(ruPath))
                        {
                            var shapeGeometry = Geometry.FromFileSystemEntry(apt, shapeEntry);
                            apt.GeometryMap[shape.GeometryId] = shapeGeometry;
                        }
                    }

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

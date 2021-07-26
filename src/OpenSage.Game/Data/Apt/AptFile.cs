using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;
using OpenSage.Data.Apt.FrameItems;

namespace OpenSage.Data.Apt
{
    public sealed class AptFile
    {
        public FileSystem FileSystem { get; }
        public ConstantData Constants { get; }
        public string MovieName { get; }

        public Movie Movie { get; private set; }
        public ImageMap ImageMap { get; private set; }
        public Dictionary<uint, Geometry> GeometryMap { get; private set; }
        public Dictionary<string, FileSystemEntry> ImportMap { get; private set; }

        internal bool IsEmpty = true;

        class _fec : IEqualityComparer<FileSystemEntry>
        {
            public bool Equals(FileSystemEntry b1, FileSystemEntry b2)
            {
                return b1.ToString().Equals(b2.ToString());
            }

            public int GetHashCode(FileSystemEntry bx)
            {
                return bx.ToString().GetHashCode();
            }
        }

        private static _fec __fec = new _fec();
        private static Dictionary<FileSystemEntry, AptFile> _cache = new Dictionary<FileSystemEntry, AptFile>(__fec);
        private static List<FileSystemEntry> _recent = new List<FileSystemEntry>();
        private static readonly int _max_count = 12;

        private AptFile(ConstantData constants, FileSystem filesystem, string name)
        {
            Constants = constants;
            FileSystem = filesystem;
            MovieName = name;
        }

        private void Parse(BinaryReader reader, string parentDirectory)
        {
            //jump to the entry offset
            var entryOffset = Constants.AptDataEntryOffset;
            reader.BaseStream.Seek(entryOffset, SeekOrigin.Begin);

            //proceed loading the characters
            Movie = (Movie) Character.Create(reader, this);

            //set first character to itself
            Movie.Characters[0] = Movie;

            //load the corresponding image map
            var datPath = Path.Combine(parentDirectory, MovieName + ".dat");
            var datEntry = FileSystem.GetFile(datPath);
            ImageMap = ImageMap.FromFileSystemEntry(datEntry);

            //resolve geometries
            GeometryMap = new Dictionary<uint, Geometry>();
            foreach (Shape shape in Movie.Characters.FindAll((x) => x is Shape))
            {
                var ruPath = Path.Combine(parentDirectory, MovieName + "_geometry", +shape.GeometryId + ".ru");
                var shapeEntry = FileSystem.GetFile(ruPath);
                var shapeGeometry = Geometry.FromFileSystemEntry(this, shapeEntry);
                GeometryMap[shape.GeometryId] = shapeGeometry;
            }

            ImportMap = new Dictionary<string, FileSystemEntry>();
            //resolve imports
            foreach (var import in Movie.Imports)
            if (!ImportMap.ContainsKey(import.Movie))
                {
                    var importPath = Path.Combine(parentDirectory, Path.ChangeExtension(import.Movie, ".apt"));
                    var importEntry = FileSystem.GetFile(importPath);
                    if(importEntry == null)
                        throw new FileNotFoundException("Cannot find imported file", importPath);
                    ImportMap[import.Movie] = importEntry;

                    // Some dirty tricks to avoid the above exception
                    FromFileSystemEntry(importEntry);
                }                               

        }

        public static AptFile FromFileSystemEntry(FileSystemEntry entry)
        {
            if (!_cache.ContainsKey(entry)) {
                _cache[entry] = FromFileSystemEntryRaw(entry);
                _recent.Add(entry);
                if (_recent.Count > _max_count)
                {
                    _cache.Remove(_recent[0]);
                    _recent.Remove(_recent[0]);
                }   
            }
            else
            {
                foreach (var v in _recent)
                    if (__fec.Equals(v, entry))
                    {
                        _recent.Remove(v);
                        break;
                    }
                _recent.Add(entry);
            }
            return _cache[entry];
        }
        public static AptFile FromFileSystemEntryRaw(FileSystemEntry entry)
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
                apt.Parse(reader, Path.GetDirectoryName(entry.FilePath));

                return apt;
            }
        }

        public static AptFile CreateEmpty(string name, int width, int height, int millisecondsPerFrame)
        {
            var constData = new ConstantData();
            var apt = new AptFile(constData, null, name)
            {
                ImageMap = new ImageMap(),
                GeometryMap = new Dictionary<uint, Geometry>(),
                ImportMap = new Dictionary<string, FileSystemEntry>(), 
        };
            apt.Movie = Movie.CreateEmpty(apt, width, height, millisecondsPerFrame);
            return apt;
        }
    }
}

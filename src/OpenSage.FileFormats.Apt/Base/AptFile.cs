using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats;
using OpenSage.FileFormats.Apt.FrameItems;

namespace OpenSage.FileFormats.Apt
{

    public abstract class AptStreamGetter
    {
        public abstract Stream GetAptStream(FileMode mode = FileMode.Open);
        public abstract Stream GetConstStream(FileMode mode = FileMode.Open);
        public abstract Stream GetDatStream(FileMode mode = FileMode.Open);
        public abstract Stream GetXmlStream(FileMode mode = FileMode.Open);
        public abstract Stream GetTextureStream(uint id, FileMode mode = FileMode.Open);
        public abstract Stream GetTextureStream2(uint id, out string path, FileMode mode = FileMode.Open);
        public abstract Stream GetGeometryStream(uint id, FileMode mode = FileMode.Open);
        public abstract string GetMovieName();
        public abstract string GetRootPath();
    }

    public class StandardStreamGetter : AptStreamGetter
    {
        public string RootPath { get; }
        public string AptName { get; }
        public Func<string, FileMode, Stream> Getter { get; }
        public override string GetMovieName() => AptName;
        public override string GetRootPath() => RootPath;

        public StandardStreamGetter(string path, string apt, Func<string, FileMode, Stream> getter = null)
        {
            RootPath = path;
            AptName = apt;
            if (getter == null) getter = File.Open;
            Getter = getter;
        }

        public StandardStreamGetter(string path, string apt, Func<string, Stream> getter) : this(
            path,
            apt,
            (getter == null ? null :
            (s, m) => { if (m == FileMode.Open) return getter(s); else throw new NotSupportedException(); }
            ))
        { }

        public StandardStreamGetter(string aptPath, Func<string, FileMode, Stream> getter = null) : this(
            Path.GetDirectoryName(aptPath), 
            Path.GetFileNameWithoutExtension(aptPath),
            getter
            )
        { }

        public override Stream GetAptStream(FileMode mode = FileMode.Open)
        {
            var path = Path.Combine(RootPath, AptName + ".apt");
            return Getter(path, mode);
        }

        public override Stream GetConstStream(FileMode mode = FileMode.Open)
        {
            var path = Path.Combine(RootPath, AptName + ".const");
            return Getter(path, mode);
        }

        public override Stream GetDatStream(FileMode mode = FileMode.Open)
        {
            var path = Path.Combine(RootPath, AptName + ".dat");
            return Getter(path, mode);
        }

        public override Stream GetGeometryStream(uint id, FileMode mode = FileMode.Open)
        {
            var path = Path.Combine(RootPath, AptName + "_geometry", +id + ".ru");
            return Getter(path, mode);
        }

        public override Stream GetTextureStream(uint id, FileMode mode = FileMode.Open)
        {
            var path = Path.Combine(RootPath, AptName + "_textures", +id + ".tga");
            return Getter(path, mode);
        }

        public override Stream GetTextureStream2(uint id, out string path, FileMode mode = FileMode.Open)
        {
            path = $"art\\Textures\\apt_{AptName}_{id}.tga";
            var path2 = Path.Combine(RootPath, path);
            return Getter(path2, mode);
        }

        public override Stream GetXmlStream(FileMode mode = FileMode.Open)
        {
            var path = Path.Combine(RootPath, AptName + ".xml");
            return Getter(path, mode);
        }
    }
    public sealed class AptFile

    {
        public string RootDirectory { get; }
        public ConstantData Constants { get; }
        public string MovieName { get; }

        public Movie Movie { get; private set; }
        public ImageMap ImageMap { get; private set; }
        public Dictionary<uint, Geometry> GeometryMap { get; private set; }
        public Dictionary<string, string> ImportMap { get; private set; }

        private AptFile(ConstantData constants, string movieName, string rootPath)
        {
            Constants = constants;
            MovieName = movieName;
            RootDirectory = rootPath;
        }

        public static AptFile Parse(string path) { return Parse(new StandardStreamGetter(path)); }
        public static AptFile Parse(AptStreamGetter getter)
        {
            using (var aptStream = getter.GetAptStream())
            using (var aptReader = new BinaryReader(aptStream, Encoding.ASCII, true))
            {
                //check if this is a valid apt file
                var fileFormat = aptReader.ReadFixedLengthString(8);
                if (fileFormat != "Apt Data")
                {
                    throw new InvalidDataException("Not an Apt file");
                }

                //load the corresponding const entry
                using (var constStream = getter.GetConstStream())
                using (var constReader = new BinaryReader(constStream))
                {
                    var constFile = ConstantData.Parse(constReader);

                    // create container & load .apt file
                    var apt = new AptFile(constFile, getter.GetMovieName(), getter.GetRootPath());
                    var entryOffset = constFile.AptDataEntryOffset;
                    aptReader.BaseStream.Seek(entryOffset, SeekOrigin.Begin);

                    //proceed loading the characters
                    apt.Movie = (Movie) Character.Create(aptReader, apt);

                    //load the corresponding image map
                    using (var datEntry = getter.GetDatStream())
                    using (var datReader = new StreamReader(datEntry))
                        apt.ImageMap = ImageMap.Parse(datReader);

                    //resolve geometries
                    apt.GeometryMap = new Dictionary<uint, Geometry>();
                    foreach (Shape shape in apt.Movie.Characters.FindAll((x) => x is Shape))
                    {
                        try
                        {
                            using (var shapeEntry = getter.GetGeometryStream(shape.GeometryId))
                            using (var shapeReader = new StreamReader(shapeEntry))
                            {
                                var shapeGeometry = Geometry.Parse(apt, shapeReader);
                                apt.GeometryMap[shape.GeometryId] = shapeGeometry;
                            }
                        }
                        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
                        {
                            apt.GeometryMap[shape.GeometryId] = null;
                        }
                    }

                    return apt;
                }
            }
        }

        public void Write(AptStreamGetter getter)
        {
            var mode = FileMode.Create;
            BinaryIOExtensions.Write(
                (w, p) => { Movie.Write(w, p, true); return -1; },
                () => getter.GetAptStream(mode)
                );
            Constants.AptDataEntryOffset = Apt.Constants.AptFileStartPos;
            BinaryIOExtensions.Write(
                (w, p) => { Constants.Write(w, p); return -1; },
                () => getter.GetConstStream(mode)
                );
            BinaryIOExtensions.Write(
                (w, p) => { ImageMap.Write(w); return -1; },
                () => getter.GetDatStream(mode)
                );
            foreach (var shapekvp in GeometryMap)
            {
                var id = shapekvp.Key;
                var shape = shapekvp.Value;
                BinaryIOExtensions.Write(
                    (w, p) => { ImageMap.Write(w); return -1; },
                    () => getter.GetDatStream(mode)
                    );
            }
        }

        public static AptFile CreateEmpty(string name, int width, int height, int millisecondsPerFrame)
        {
            var constData = new ConstantData();
            var apt = new AptFile(constData, name, string.Empty)
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

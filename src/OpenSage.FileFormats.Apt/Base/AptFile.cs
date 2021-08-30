using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats;
using OpenSage.FileFormats.Apt.FrameItems;
using System.Threading.Tasks;

namespace OpenSage.FileFormats.Apt
{
    public enum DataType
    {
        Apt,
        Const,
        Dat,
        Xml,
        Texture,
        Texture2,
        Geometry
    }

    public abstract class AptStreamGetter
    {
        public string GetPath(DataType type, uint id) { return GetPath(type, false, id); }
        public abstract string GetPath(DataType type, bool addRootPath = false, uint id = 0);
        public abstract Stream GetStream(DataType type, uint id = 0, FileMode mode = FileMode.Open);
        public Stream GetStream(DataType type, FileMode mode) { return GetStream(type, 0, mode); }
        public abstract string GetMovieName();
        public abstract string GetRootPath();
        public abstract IEnumerable<string> EnsureDirectory();
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

        public override string GetPath(DataType type, bool addRootPath = false, uint id = 0)
        {
            var path = "";
            switch (type)
            {
                case DataType.Apt:
                    path = AptName + ".apt";
                    break;
                case DataType.Const:
                    path = AptName + ".const";
                    break;
                case DataType.Dat:
                    path = AptName + ".dat";
                    break;
                case DataType.Xml:
                    path = AptName + ".xml";
                    break;
                case DataType.Geometry:
                    if (id == 0)
                        throw new ArgumentException();
                    path = Path.Combine(AptName + "_geometry", +id + ".ru");
                    break;
                case DataType.Texture:
                    if (id == 0)
                        throw new ArgumentException();
                    path = Path.Combine(AptName + "_textures", +id + ".tga");
                    break;
                case DataType.Texture2:
                    if (id == 0)
                        throw new ArgumentException();
                    path = $"art\\Textures\\apt_{AptName}_{id}.tga";
                    break;
            }
            if (addRootPath)
                path = Path.Combine(RootPath, path);
            return path;
        }

        public override Stream GetStream(DataType type, uint id = 0, FileMode mode = FileMode.Open)
        {
            var path = GetPath(type, true, id);
            return Getter(path, mode);
        }

        public override IEnumerable<string> EnsureDirectory()
        {
            List<string> ans = new()
            {
                RootPath,
                Path.Combine(RootPath, AptName + "_textures"),
                Path.Combine(RootPath, AptName + "_geometry"),
                Path.Combine(RootPath, "art\\Textures"),
            };
            return ans;
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
            using (var aptStream = getter.GetStream(DataType.Apt))
            using (var aptReader = new BinaryReader(aptStream, Encoding.ASCII, true))
            {
                //check if this is a valid apt file
                var fileFormat = aptReader.ReadFixedLengthString(8);
                if (fileFormat != "Apt Data")
                {
                    throw new InvalidDataException("Not an Apt file");
                }

                //load the corresponding const entry
                using (var constStream = getter.GetStream(DataType.Const))
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
                    using (var datEntry = getter.GetStream(DataType.Dat))
                    using (var datReader = new StreamReader(datEntry))
                        apt.ImageMap = ImageMap.Parse(datReader);

                    //resolve geometries
                    apt.GeometryMap = new Dictionary<uint, Geometry>();
                    foreach (Shape shape in apt.Movie.Characters.FindAll((x) => x is Shape))
                    {
                        try
                        {
                            using (var shapeEntry = getter.GetStream(DataType.Geometry, shape.GeometryId))
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
            foreach (var dir in getter.EnsureDirectory())
                Directory.CreateDirectory(dir);
            
            BinaryIOExtensions.Write(
                (w, p) => { Movie.Write(w, p, true); return -1; },
                () => getter.GetStream(DataType.Apt, mode)
                );
            Constants.AptDataEntryOffset = Apt.Constants.AptFileStartPos;
            BinaryIOExtensions.Write(
                (w, p) => { Constants.Write(w, p); return -1; },
                () => getter.GetStream(DataType.Const, mode)
                );
            BinaryIOExtensions.Write(
                (w, p) => { ImageMap.Write(w); return -1; },
                () => getter.GetStream(DataType.Dat, mode)
                );

            foreach (var shapekvp in GeometryMap)
            {
                var id = shapekvp.Key;
                var shape = shapekvp.Value.RawText;
                using var g = getter.GetStream(DataType.Geometry, id, mode);
                using var s = new StreamWriter(g);
                s.Write(shape);
            }
        }

        public async Task WriteAsync(AptStreamGetter getter)
        {
            var mode = FileMode.Create;
            foreach (var dir in getter.EnsureDirectory())
                Directory.CreateDirectory(dir);

            var tasks = new List<Task>();

            tasks.Add(Task.Run(() => BinaryIOExtensions.Write(
                (w, p) => { Movie.Write(w, p, true); return -1; },
                () => getter.GetStream(DataType.Apt, mode)
                )));
            Constants.AptDataEntryOffset = Apt.Constants.AptFileStartPos;
            tasks.Add(Task.Run(() => BinaryIOExtensions.Write(
                (w, p) => { Constants.Write(w, p); return -1; },
                () => getter.GetStream(DataType.Const, mode)
                )));
            tasks.Add(Task.Run(() => BinaryIOExtensions.Write(
                (w, p) => { ImageMap.Write(w); return -1; },
                () => getter.GetStream(DataType.Dat, mode)
                )));

            foreach (var shapekvp in GeometryMap)
            {
                var id = shapekvp.Key;
                var shape = shapekvp.Value.RawText;
                using var g = getter.GetStream(DataType.Geometry, id, mode);
                using var s = new StreamWriter(g);
                tasks.Add(s.WriteAsync(shape));
            }

            await Task.WhenAll(tasks);
        }

        public void WriteTextures(AptStreamGetter getter, AptStreamGetter sourceGetter = null)
        {
            if (sourceGetter == null)
                sourceGetter = new StandardStreamGetter(RootDirectory, MovieName);
            foreach (var texId in ImageMap.Mapping.Values
                .Select(m => m.TextureId)
                .Distinct())
            {
                var filePath = getter.GetPath(DataType.Texture2, (uint) texId);
                var directory = Path.GetDirectoryName(filePath);
                if (directory is string)
                    Directory.CreateDirectory(directory);

                using (var memory = new MemoryStream())
                {
                    using var output = getter.GetStream(DataType.Texture2, (uint) texId, FileMode.Create);
                    using var input = sourceGetter.GetStream(DataType.Texture2, (uint) texId, FileMode.Create);
                    input.CopyTo(memory);
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.CopyTo(output);
                }
            }
        }

        public void GenerateXml(AptStreamGetter getter)
        {
            var Name = getter.GetMovieName();
            var rootPath = getter.GetRootPath();
            // TODO: use a true XML serializer
            using var xml = new StreamWriter(getter.GetStream(DataType.Xml, FileMode.Create));

            // head
            xml.WriteLine("<?xml version='1.0' encoding='utf-8'?>");
            xml.WriteLine($"<!--{Apt.Constants.OpenSageAptEditorCredits}-->");
            xml.WriteLine("<AssetDeclaration xmlns=\"uri:ea.com:eala:asset\">");

            // apt
            var list = new[] { DataType.Apt, DataType.Const, DataType.Dat };
            foreach (var type in list)
            {
                var extension = type.ToString().ToLowerInvariant();
                var fileName = getter.GetPath(type);
                xml.WriteLine($"    <Apt{type}Data id=\"{Name}_{extension}\" File=\"{fileName}\" />");
            }

            // geometry
            foreach (var (id, _) in GeometryMap)
            {
                var ruName = getter.GetPath(DataType.Geometry, id);
                xml.WriteLine($"    <AptGeometryData id=\"{Name}_{id}\" File=\"{ruName}\" AptID=\"{id}\" />");
            }

            // textures
            foreach (var texId in ImageMap.Mapping.Values
                .Select(m => m.TextureId)
                .Distinct())
            {
                var filePath = getter.GetPath(DataType.Texture2, (uint) texId);
                const string options = "OutputFormat=\"A8R8G8B8\" GenerateMipMaps=\"false\" AllowAutomaticResize=\"false\"";
                xml.WriteLine($"    <Texture id=\"{texId}\" File=\"{filePath}\" {options} />");
            }

            xml.WriteLine("</AssetDeclaration>");
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

using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Apt
{
    public sealed class AptFile
    {
        public ConstantData Constants { get; private set; }
        public Movie Movie { get; private set; }
        internal bool IsEmpty = true;
        internal string MovieName;
        internal string BaseUrl;
        internal ImageMap ImageMap;
        internal Dictionary<uint, Geometry> GeometryMap;

        private AptFile(ConstantData constants, string baseUrl, string name)
        {
            Constants = constants;
            BaseUrl = baseUrl;
            MovieName = name;
        }

        private static bool IsShape(Character x)
        {
            return x is Shape;
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
            var datUrl = FileSystem.Combine(BaseUrl, MovieName + ".dat");
            ImageMap = ImageMap.FromUrl(datUrl);

            //resolve geometries
            GeometryMap = new Dictionary<uint, Geometry>();
            foreach (Shape shape in Movie.Characters.FindAll(IsShape))
            {
                var shapeGeometry = Geometry.FromUrl(FileSystem.Combine(BaseUrl, MovieName + "_geometry", shape.Geometry + ".ru"));
                shapeGeometry.Container = this;
                GeometryMap[shape.Geometry] = shapeGeometry;
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
                    importApt = FromUrl(FileSystem.Combine(BaseUrl, FileSystem.GetParentFolder(import.Movie), FileSystem.GetFileNameWithoutExtension(import.Movie) + ".apt"));
                    importDict[import.Movie] = importApt;
                }

                //get the export from that apt and proceed
                var export = importApt.Movie.Exports.Find(x => x.Name == import.Name);

                //place the exported character inside our movie
                Movie.Characters[(int) import.Character] = importApt.Movie.Characters[(int) export.Character];
            }
        }

        public static AptFile FromUrl(string url)
        {
            using (var stream = FileSystem.OpenStream(url, IO.FileMode.Open))
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                //check if this is a valid apt file
                var magic = reader.ReadFixedLengthString(8);
                if (magic != "Apt Data")
                {
                    throw new InvalidDataException();
                }

                var baseUrl = FileSystem.GetParentFolder(url);
                var aptName = FileSystem.GetFileNameWithoutExtension(url);

                //load the corresponding const entry
                var constFile = ConstantData.FromUrl(FileSystem.Combine(baseUrl, aptName + ".const"));

                var apt = new AptFile(constFile, baseUrl, aptName);
                apt.Parse(reader);

                return apt;
            }
        }
    }
}

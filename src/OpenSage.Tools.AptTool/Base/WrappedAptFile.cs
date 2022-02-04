using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Base;
using OpenSage.FileFormats.Apt;

namespace OpenSage.Tools.AptTool.Base
{
    public record WrappedPackageInfo
    {
        public string PackagePath;
        public string ObjectName;
        public int TargetCharacter;

    }

    public record WrappedAptFile
    {
        public string FileName;

        public string MovieName;

        public List<ConstantEntry> Constants;

        public List<WrappedCharacter> Characters;

        public List<string> Imports;

        public List<string> Exports;

        // TODO geometry

        // TODO image

        public WrappedAptFile()
        {

        }
        /*
        public WrappedAptFile(AptFile f)
        {
            FileName = f.RootDirectory;
            MovieName = f.MovieName;

            Constants = new(f.Constants.Entries);

            Characters = new();
            Imports = new();
            Exports = new();

            // TODO geometry & image
            // just use original ones? not always possible

            // build vars in character
            foreach (var i in f.Movie.Imports)
                Imports.Add(i);
        }
        */
    }
}

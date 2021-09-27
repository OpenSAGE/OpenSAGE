using System;
using System.IO;
using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.FileFormats.Apt;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor
{

    public sealed class AptEditInstance: EditManager
    {
        public AptFile AptFile { get; }

        public AptEditInstance(AptFile aptFile): base()
        {
            AptFile = aptFile;
        }

        // Not Recommended?
        public static AptEditInstance Load(string rootPath, string aptPath)
        {
            var fs = new FileSystem(rootPath);
            var entry = fs.GetFile(aptPath);
            return new AptEditInstance(AptFileHelper.FromFileSystemEntry(entry));
        }


        public AptDataDump GetAptDataDump(AptStreamGetter? sourceGetter = null)
        {
            return new AptDataDump(AptFile, sourceGetter);
        }

        public AptDataDump GetAptDataDump(Func<string, FileMode, Stream> get)
        {
            var source = new StandardStreamGetter(AptFile.RootDirectory, AptFile.MovieName, get);
            return new AptDataDump(AptFile, source);
        }
    }

}

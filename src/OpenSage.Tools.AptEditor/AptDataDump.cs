using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenSage.FileFormats.Apt;
using System.Threading.Tasks;

namespace OpenSage.Tools.AptEditor
{
    public sealed class AptDataDump
    {
        public AptFile Apt { get; }
        public string Name { get; }
        public AptStreamGetter? SourceGetter { get; }

        public AptDataDump(AptFile file, AptStreamGetter? source = null)
        {
            // TODO of course
            Apt = file;
            Name = file.MovieName;
            SourceGetter = source;
        }

        public async Task WriteTo(string exportPath)
        {
            var getter = new StandardStreamGetter(exportPath, Name);
            var tasks = new[]
            {
                Apt.WriteAsync(getter),
                Task.Run(() => Apt.GenerateXml(getter)),
                Task.Run(() => Apt.WriteTextures(getter, SourceGetter!))
            };
            await Task.WhenAll(tasks);
        }

        public async Task WriteTo(AptStreamGetter getter)
        {
            var tasks = new[]
            {
                Apt.WriteAsync(getter),
                Task.Run(() => Apt.GenerateXml(getter)),
                Task.Run(() => Apt.WriteTextures(getter, SourceGetter!))
            };
            await Task.WhenAll(tasks);
        }

    }

}

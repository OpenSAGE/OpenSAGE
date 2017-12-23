using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace OpenSage.Data.Apt
{
    public sealed class Geometry
    {
        public static Geometry FromFileSystemEntry(FileSystemEntry entry)
        {
            var geometry = new Geometry();

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line=reader.ReadLine())!=null)
                {
                    
                }
            }

            return geometry;
        }
    }
}

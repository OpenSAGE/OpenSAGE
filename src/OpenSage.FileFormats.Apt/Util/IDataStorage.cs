using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.FileFormats.Apt
{
    public interface IDataStorage
    {
        public abstract void Write(BinaryWriter writer, MemoryPool pool);
    }


}

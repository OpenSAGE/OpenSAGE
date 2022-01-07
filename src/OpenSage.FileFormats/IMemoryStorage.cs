using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.FileFormats
{
    public interface IMemoryStorage
    {
        // DO NOT ALIGN A STORAGE ITSELF INSIDE THIS FUNCTION!
        public abstract void Write(BinaryWriter writer, BinaryMemoryChain memory);
    }


}

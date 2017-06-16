using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pfim
{
    /// <summary>Describes how pixel data is arranged</summary>
    public enum ImageFormat
    {
        /// <summary>Red, green, and blue channels are 8 bits apiece</summary>
        Rgb24,

        /// <summary>
        /// Red, green, blue, and alpha are 8 bits apiece
        /// </summary>
        Rgba32
    }
}

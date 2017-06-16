using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pfim
{
    /// <summary>
    /// Defines a common interface that all images are decoded into
    /// </summary>
    public interface IImage
    {
        /// <summary>The raw data</summary>
        byte[] Data { get; }

        /// <summary>Width of the image in pixels</summary>
        int Width { get; }

        /// <summary>Height of the image in pixels</summary>
        int Height { get; }

        /// <summary>The number of bytes that compose one line</summary>
        int Stride { get; }

        /// <summary>The format of the raw data</summary>
        ImageFormat Format { get; }
    }
}

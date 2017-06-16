using System.IO;

namespace Pfim
{
    /// <summary>
    /// Interface describe a class that is able to decode a targa image that may have its pixel
    /// start anywhere. For instance, in a bottom left scenario, the first pixel read corresponds to
    /// the first pixel on the last row
    /// </summary>
    internal interface IDecodeTarga
    {
        /// <summary>Decode pixels starting at bottom left</summary>
        byte[] BottomLeft(Stream str, TargaHeader header);

        /// <summary>Decode pixels starting at bottom right</summary>
        byte[] BottomRight(Stream str, TargaHeader header);

        /// <summary>Decode pixels starting at top right</summary>
        byte[] TopRight(Stream str, TargaHeader header);

        /// <summary>Decode pixels starting at top left</summary>
        byte[] TopLeft(Stream str, TargaHeader header);
    }
}
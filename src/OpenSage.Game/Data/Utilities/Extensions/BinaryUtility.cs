using System.IO;
using System.Text;

namespace OpenSage.Data.Utilities.Extensions
{
    internal static class BinaryUtility
    {
        static BinaryUtility()
        {
            AnsiEncoding = Encoding.GetEncoding(1252);
        }

        /// <summary>
        /// It should be Encoding.ASCII, but Generals uses 0xA0 (non-breaking space) 
        /// which is in the extended ASCII character set.
        /// </summary>
        public static Encoding AnsiEncoding { get; }
    }

    internal abstract class BinaryObject
    {
        public BinaryObject(BinaryReader br)
        {

        }
    }
}

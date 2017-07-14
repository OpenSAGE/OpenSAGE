using System.Text;

namespace OpenZH.Data.Utilities.Extensions
{
    internal static class BinaryUtility
    {
        static BinaryUtility()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            AnsiEncoding = Encoding.GetEncoding(1252);
        }

        /// <summary>
        /// It should be Encoding.ASCII, but Generals uses 0xA0 (non-breaking space) 
        /// which is in the extended ASCII character set.
        /// </summary>
        public static Encoding AnsiEncoding { get; }
    }
}

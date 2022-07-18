using System.Text;

namespace OpenSage.FileFormats
{
    public static class BinaryUtility
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

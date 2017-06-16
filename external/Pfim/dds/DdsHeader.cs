using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Pfim
{
    /// <summary>
    /// Denotes the compression algorithm used in the image. Either the image
    /// is uncompressed or uses some sort of block compression. The
    /// compression used is encoded in the header of image as textual
    /// representation of itself. So a DXT1 image is encoded as "1TXD" so the
    /// enum represents these values directly
    /// </summary>
    internal enum CompressionAlgorithm : uint
    {
        /// <summary>
        /// No compression was used in the image.
        /// </summary>
        None = 0,

        /// <summary>
        /// <see cref="Dxt1Dds"/>. Also known as BC1
        /// </summary>
        D3DFMT_DXT1 = 827611204,

        /// <summary>
        /// Not supported. Also known as BC2
        /// </summary>
        D3DFMT_DXT2 = 844388420,

        /// <summary>
        /// <see cref="Dxt3Dds"/>. Also known as BC3
        /// </summary>
        D3DFMT_DXT3 = 861165636,

        /// <summary>
        /// Not supported. Also known as BC4
        /// </summary>
        D3DFMT_DXT4 = 877942852,

        /// <summary>
        /// <see cref="Dxt5Dds"/>. Also known as BC5
        /// </summary>
        D3DFMT_DXT5 = 894720068
    }

    /// <summary>Flags to indicate which members contain valid data.</summary>
    [Flags]
    internal enum DdsFlags : uint
    {
        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        Caps = 0x1,

        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        Height = 0x2,

        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        Width = 0x4,

        /// <summary>
        /// Required when pitch is provided for an uncompressed texture.
        /// </summary>
        Pitch = 0x8,

        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        PixelFormat = 0x1000,

        /// <summary>
        /// Required in a mipmapped texture.
        /// </summary>
        MipMapCount = 0x20000,

        /// <summary>
        /// Required when pitch is provided for a compressed texture.
        /// </summary>
        LinearSize = 0x80000,

        /// <summary>
        /// Required in a depth texture.
        /// </summary>
        Depth = 0x800000
    }

    /// <summary>
    /// Surface pixel format.
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/bb943984(v=vs.85).aspx
    /// </summary>
    internal struct DdsPixelFormat
    {
        /// <summary>
        /// Structure size; set to 32 (bytes).
        /// </summary>
        public uint Size;

        /// <summary>
        /// Values which indicate what type of data is in the surface. 
        /// </summary>
        public uint Flags;

        /// <summary>
        /// Four-character codes for specifying compressed or custom formats.
        /// Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5.  A
        /// FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10
        /// extended header,  and the dxgiFormat member of that structure
        /// indicates the true format. When using a four-character code,
        /// dwFlags must include DDPF_FOURCC.
        /// </summary>
        public CompressionAlgorithm FourCC;

        /// <summary>
        /// Number of bits in an RGB (possibly including alpha) format.
        /// Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.
        /// </summary>
        public uint RGBBitCount;

        /// <summary>
        /// Red (or lumiannce or Y) mask for reading color data.
        /// For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
        /// </summary>
        public uint RBitMask;

        /// <summary>
        /// Green (or U) mask for reading color data.
        /// For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
        /// </summary>
        public uint GBitMask;

        /// <summary>
        /// Blue (or V) mask for reading color data.
        /// For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
        /// </summary>
        public uint BBitMask;

        /// <summary>
        /// Alpha mask for reading alpha data. 
        /// dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. 
        /// For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
        /// </summary>
        public uint ABitMask;
    }

    /// <summary>
    /// The header that accompanies all direct draw images
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/bb943982(v=vs.85).aspx
    /// </summary>
    internal class DdsHeader
    {
        /// <summary>
        /// Size of a Direct Draw Header in number of bytes.
        /// This does not include the magic number
        /// </summary>
        public const int SIZE = 124;

        /// <summary>
        /// The magic number is the 4 bytes that starts off every Direct Draw Surface file.
        /// </summary>
        const uint DDS_MAGIC = 542327876;

        DdsPixelFormat pixelFormat;

        /// <summary>Create header from stream</summary>
        public DdsHeader(Stream stream)
        {
            headerInit(stream);
        }

        private unsafe void headerInit(Stream stream)
        {
            byte[] buffer = new byte[SIZE + 4];
            Reserved1 = new uint[11];
            int bufferSize, workingSize;
            bufferSize = workingSize = stream.Read(buffer, 0, SIZE + 4);

            fixed (byte* bufferPtr = buffer)
            {
                uint* workingBufferPtr = (uint*)bufferPtr;
                if (*workingBufferPtr++ != DDS_MAGIC)
                    throw new Exception("Not a valid DDS");
                if ((Size = *workingBufferPtr++) != SIZE)
                    throw new Exception("Not a valid header size");
                Flags = (DdsFlags)(*workingBufferPtr++);
                Height = *workingBufferPtr++;
                Width = *workingBufferPtr++;
                PitchOrLinearSize = *workingBufferPtr++;
                Depth = *workingBufferPtr++;
                MipMapCout = *workingBufferPtr++;
                fixed (uint* reservedPtr = Reserved1)
                {
                    uint* workingReservedPtr = reservedPtr;
                    for (int i = 0; i < 11; i++)
                        *workingReservedPtr++ = *workingBufferPtr++;
                }

                pixelFormat.Size = *workingBufferPtr++;
                pixelFormat.Flags = *workingBufferPtr++;
                pixelFormat.FourCC = (CompressionAlgorithm)(*workingBufferPtr++);
                pixelFormat.RGBBitCount = *workingBufferPtr++;
                pixelFormat.RBitMask = *workingBufferPtr++;
                pixelFormat.GBitMask = *workingBufferPtr++;
                pixelFormat.BBitMask = *workingBufferPtr++;
                pixelFormat.ABitMask = *workingBufferPtr++;

                Caps = *workingBufferPtr++;
                Caps2 = *workingBufferPtr++;
                Caps3 = *workingBufferPtr++;
                Caps4 = *workingBufferPtr++;
                Reserved2 = *workingBufferPtr++;
            }
        }

        /// <summary>
        /// Size of structure. This member must be set to 124.
        /// </summary>
        public uint Size { get; private set; }

        /// <summary>
        /// Flags to indicate which members contain valid data. 
        /// </summary>
        DdsFlags Flags { get;  set; }

        /// <summary>
        /// Surface height in pixels
        /// </summary>
        public uint Height { get; private set; }

        /// <summary>
        /// Surface width in pixels
        /// </summary>
        public uint Width { get; private set; }

        /// <summary>
        /// The pitch or number of bytes per scan line in an uncompressed texture.
        /// The total number of bytes in the top level texture for a compressed texture.
        /// </summary>
        public uint PitchOrLinearSize { get; private set; }

        /// <summary>
        /// Depth of a volume texture (in pixels), otherwise unused. 
        /// </summary>
        public uint Depth { get; private set; }

        /// <summary>
        /// Number of mipmap levels, otherwise unused.
        /// </summary>
        public uint MipMapCout { get; private set; }

        /// <summary>
        /// Unused
        /// </summary>
        public uint[] Reserved1 { get; private set; }

        /// <summary>
        /// The pixel format 
        /// </summary>
        public DdsPixelFormat PixelFormat { get { return pixelFormat ;} }

        /// <summary>
        /// Specifies the complexity of the surfaces stored.
        /// </summary>
        public uint Caps { get; private set; }

        /// <summary>
        /// Additional detail about the surfaces stored.
        /// </summary>
        public uint Caps2 { get; private set; }

        /// <summary>
        /// Unused
        /// </summary>
        public uint Caps3 { get; private set; }

        /// <summary>
        /// Unused
        /// </summary>
        public uint Caps4 { get; private set; }

        /// <summary>
        /// Unused
        /// </summary>
        public uint Reserved2 { get; private set; }
    }
}

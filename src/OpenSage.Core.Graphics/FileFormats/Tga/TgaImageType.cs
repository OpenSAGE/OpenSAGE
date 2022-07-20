namespace OpenSage.Data.Tga
{
    public enum TgaImageType : byte
    {
        NoImageData = 0,
        UncompressedColorMapped = 1,
        UncompressedRgb = 2,
        UncompressedBlackAndWhite = 3,
        RunLengthEncodedColorMapped = 9,
        RunLengthEncodedRgb = 10,
        CompressedBlackAndWhite = 11,
        CompressedColorMapped = 32,
        CompressedColorMapped4PassQuadTree = 33
    }
}

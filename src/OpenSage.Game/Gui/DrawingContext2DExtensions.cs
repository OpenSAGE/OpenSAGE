using OpenSage.Mathematics;

namespace OpenSage.Gui;

public static class DrawingContext2DExtensions
{
    public static void DrawMappedImage(
        this DrawingContext2D drawingContext,
        MappedImage mappedImage,
        in RectangleF destinationRect,
        bool flipped = false,
        bool grayscaled = false)
    {
        drawingContext.DrawImage(
            mappedImage.Texture.Value,
            mappedImage.Coords,
            destinationRect,
            flipped,
            grayscaled);
    }
}

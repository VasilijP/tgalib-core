namespace tgalib_core;

// Holds decoded image data. This needs platform specific conversion to the actual image/texture format.
public class Image(TgaHeader header, TgaFooter footer, PixelFormat pixelFormat, byte[] pixelData)
{
    public TgaHeader Header { get; } = header;
    public TgaFooter Footer { get; } = footer;
    public int Width { get; } = header.Width;
    public int Height { get; } = header.Height;
    public PixelFormat PixelFormat { get; } = pixelFormat;
    public byte[] PixelData { get; } = pixelData;
}

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
    
    
    // X Origin of Image: Integer ( lo-hi ) X coordinate of the lower left corner of the image.
    // Y Origin of Image: Integer ( lo-hi ) Y coordinate of the lower left corner of the image.
    public void GetPixelRgba(int x, int y, out int r, out int g, out int b, out int a)
    {
        int index = y * Width + x;

        switch (PixelFormat)
        {
            case PixelFormat.Bgra32:
            {
                int byteIndex = index * 4; // 4 bytes per pixel
                b = PixelData[byteIndex];
                g = PixelData[byteIndex + 1];
                r = PixelData[byteIndex + 2];
                a = PixelData[byteIndex + 3];
                break;
            }
            case PixelFormat.Bgr24:
            {
                int byteIndex = index * 3; // 3 bytes per pixel
                b = PixelData[byteIndex];
                g = PixelData[byteIndex + 1];
                r = PixelData[byteIndex + 2];
                a = 255; // Fully opaque
                break;
            }
            case PixelFormat.Bgr555:
            {
                int byteIndex = index * 2; // 2 bytes per pixel
                ushort value = (ushort)(PixelData[byteIndex] | (PixelData[byteIndex + 1] << 8));

                // Bits: x RRRRR GGGGG BBBBB
                int r5 = (value >> 10) & 0x1F;
                int g5 = (value >> 5) & 0x1F;
                int b5 = value & 0x1F;

                // Expand 5-bit values to 8-bit
                r = (r5 << 3) | (r5 >> 2);
                g = (g5 << 3) | (g5 >> 2);
                b = (b5 << 3) | (b5 >> 2);
                a = 255; // Fully opaque
                break;
            }
            case PixelFormat.Gray8:
            {
                int v = PixelData[index];
                r = g = b = v;
                a = 255; // Fully opaque
                break;
            }
            default: throw new NotSupportedException($"Pixel format {PixelFormat} is not supported.");
        }
    }

}

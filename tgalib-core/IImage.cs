namespace tgalib_core;

/// <summary>
/// Abstraction of the image usable for simplified loading and saving routines.
/// </summary>
public interface IImage
{
    /// <summary>
    /// Width of the image in pixels.
    /// </summary>
    int Width { get; }
    
    /// <summary>
    /// Height of the image in pixels.
    /// </summary>
    int Height { get; }
    
    /// <summary>
    /// Pixel access.
    /// </summary>
    /// <param name="x">X coordinate of a pixel within the image.</param>
    /// <param name="y">Y coordinate of a pixel within the image. Note: TGA coordinate [0, 0] is a bottom left corner.</param>
    /// <param name="r">Red component (8 bit).</param>
    /// <param name="g">Green component (8 bit).</param>
    /// <param name="b">Blue component (8 bit).</param>
    /// <param name="a">Alpha component (8 bit).</param>
    void GetPixelRgba(int x, int y, out int r, out int g, out int b, out int a);
}
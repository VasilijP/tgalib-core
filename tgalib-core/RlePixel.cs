namespace tgalib_core;

// Pixel value with repetition count.
public class RlePixel(byte[] arg)
{
    // data for a single pixel
    public readonly byte[] C = arg;
    
    // repetition count (on top of the single pixel)
    public int Rep = 0; 
}

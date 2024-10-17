# TgaLib core

- Forked from *shns/TgaLib:master* in order to upgrade project file for .Net 8
- Removed platform specific parts so this could work on non-Windows OS.
- Added my old (2004 / Java) implementation for saving&loading of 24bit RGB TGA images.

# TgaLib

--------
TgaLib is a library to decode TARGA image format.

Following features are support:
- RLE and raw TARGA images with 8/15/16/24/32 bits per pixel,
  monochrome, truecolor and colormapped images.
- Image origins, attribute type in extensions area.

## Example
----------
```C#
// Load the original TGA image
TgaImage actualImage = new("test_8bit.tga");

// Save the image using the specified TgaMode
using FileStream fs = File.Create("test03.tga");
TgaFileFormat.CommonSave(TgaMode.Pal8Rle, fs, actualImage);

// Reload the saved image
TgaImage actualImage2 = new("test03.tga");

// Compare dimensions
int width = actualImage.Width;
int height = actualImage.Height;

Assert.That(width, Is.EqualTo(actualImage2.Width));
Assert.That(height, Is.EqualTo(actualImage2.Height));

// Compare pixel data
for (int x = 0; x < width; x++)
{
    for (int y = 0; y < height; y++)
    {
        actualImage.GetPixelRgba(x, y, out int r1, out int g1, out int b1, out int a1);
        actualImage2.GetPixelRgba(x, y, out int r2, out int g2, out int b2, out int a2);
        Assert.That(r1, Is.EqualTo(r2), $"Red component mismatch at ({x}, {y})");
        Assert.That(g1, Is.EqualTo(g2), $"Green component mismatch at ({x}, {y})");
        Assert.That(b1, Is.EqualTo(b2), $"Blue component mismatch at ({x}, {y})");
    }
}
```

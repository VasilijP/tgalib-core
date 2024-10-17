using System.IO;
using NUnit.Framework;

namespace tgalib_core.Tests;

[TestFixture]
public class TgaFileFormatTests
{
    [Test]
    [TestCase("resources/CBW8.TGA")]
    public void Test01_LoadSaveRgb24Rle(string filename)
    {
        TgaImage actualImage = new(filename);
        using FileStream fs = File.OpenWrite("test01.tga");
        TgaFileFormat.CommonSave(TgaMode.Rgb24Rle, fs, actualImage);
    }
    
    [Test]
    [TestCase("resources/UBW8.TGA", false)]
    [TestCase("resources/UCM8.TGA", false)]
    [TestCase("resources/UTC16.TGA", false)]
    [TestCase("resources/UTC24.TGA", false)]
    [TestCase("resources/UTC32.TGA", false)]
    [TestCase("resources/UBW8.TGA", true)]
    [TestCase("resources/CBW8.TGA", false)]
    [TestCase("resources/CCM8.TGA", false)]
    [TestCase("resources/CTC16.TGA", false)]
    [TestCase("resources/CTC24.TGA", false)]
    [TestCase("resources/CTC32.TGA",  false)]
    [TestCase("resources/rgb32rle.tga", true)]
    [TestCase("resources/test_24bit.tga", true)]
    [TestCase("resources/test_8bit.tga", false)]
    [TestCase("resources/test_24bitRLE.tga", false)]
    public void Test02_RoundTripRgb24Rle(string filename, bool forceAlpha)
    {
        TgaImage actualImage = new(filename, forceAlpha);
        
        using FileStream fs = File.Create("test02.tga");
        TgaFileFormat.CommonSave(TgaMode.Rgb24Rle, fs, actualImage);
        
        TgaImage actualImage2 = new("test02.tga", forceAlpha);
        
        int width = actualImage.Width;
        int height = actualImage.Height;
        
        Assert.That(width, Is.EqualTo(actualImage2.Width));
        Assert.That(height, Is.EqualTo(actualImage2.Height));
        
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
    }
    
    [Test]
    [TestCase("resources/UBW8.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/UCM8.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/UTC16.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/UTC24.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/UTC32.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/UBW8.TGA", TgaMode.Pal8Unc, true)]
    [TestCase("resources/CBW8.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/CCM8.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/CTC16.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/CTC24.TGA", TgaMode.Pal8Unc, false)]
    [TestCase("resources/CTC32.TGA", TgaMode.Pal8Unc,  false)]
    [TestCase("resources/test_8bit.tga", TgaMode.Pal8Unc, false)]
    [TestCase("resources/test_24bitRLE.tga", TgaMode.Pal8Unc, false)]
    [TestCase("resources/test_8bitRLE.tga", TgaMode.Pal8Unc, false)]
    [TestCase("resources/UBW8.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/UCM8.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/UTC16.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/UTC24.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/UTC32.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/UBW8.TGA", TgaMode.Pal8Rle, true)]
    [TestCase("resources/CBW8.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/CCM8.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/CTC16.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/CTC24.TGA", TgaMode.Pal8Rle, false)]
    [TestCase("resources/CTC32.TGA", TgaMode.Pal8Rle,  false)]
    [TestCase("resources/test_8bit.tga", TgaMode.Pal8Rle, false)]
    [TestCase("resources/test_24bitRLE.tga", TgaMode.Pal8Rle, false)]
    [TestCase("resources/test_8bitRLE.tga", TgaMode.Pal8Rle, false)]
    public void Test03_RoundTripPal8(string filename, TgaMode mode, bool forceAlpha)
    {
        // Load the original TGA image
        TgaImage actualImage = new(filename, forceAlpha);

        // Save the image using the specified TgaMode
        using FileStream fs = File.Create("test03.tga");
        TgaFileFormat.CommonSave(mode, fs, actualImage);

        // Reload the saved image
        TgaImage actualImage2 = new("test03.tga", forceAlpha);

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
    }
}
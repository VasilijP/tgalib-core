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
        using FileStream ts = new(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader r = new(ts);
        TgaImage tga = new(r);
        Image actualImage = tga.GetImage();
        
        TgaFileFormat tff = new();
        using FileStream fs = File.OpenWrite("test01.tga");
        tff.CommonSave(TgaMode.Rgb24Rle, fs, actualImage);
        fs.Close();
        // Assert.Pass();
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
    public void Test02_RoundTripRgb24Rle(string filename, bool forceAlpha)
    {
        using FileStream ts = new(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader br = new(ts);
        TgaImage tga = new(br, forceAlpha);
        Image actualImage = tga.GetImage();

        TgaFileFormat tff = new();
        using FileStream fs = File.OpenWrite("test02.tga");
        tff.CommonSave(TgaMode.Rgb24Rle, fs, actualImage);
        fs.Close();

        using FileStream ts2 = new("test02.tga", FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader br2 = new(ts2);
        TgaImage tga2 = new(br2, forceAlpha);
        Image actualImage2 = tga2.GetImage();
        
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
                Assert.That(r1, Is.EqualTo(r2));
                Assert.That(g1, Is.EqualTo(g2));
                Assert.That(b1, Is.EqualTo(b2));
            }
        }
    }
}
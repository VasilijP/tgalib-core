using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using tgalib_core;

namespace tgalib_core.Tests;

[TestFixture]
[TestOf(typeof(TgaImage))]
public class TgaImageTest
{
    [Test]
    [TestCase("resources/UBW8.TGA", "resources/grayscale.png", false)]
    [TestCase("resources/UCM8.TGA", "resources/color.png", false)]
    [TestCase("resources/UTC16.TGA", "resources/color.png", false)]
    [TestCase("resources/UTC24.TGA", "resources/color.png", false)]
    [TestCase("resources/UTC32.TGA", "resources/color.png", false)]
    [TestCase("resources/UBW8.TGA", "resources/grayscale.png", true)]
    [TestCase("resources/CBW8.TGA", "resources/grayscale.png", false)]
    [TestCase("resources/CCM8.TGA", "resources/color.png", false)]
    [TestCase("resources/CTC16.TGA", "resources/color.png", false)]
    [TestCase("resources/CTC24.TGA", "resources/color.png", false)]
    [TestCase("resources/CTC32.TGA", "resources/color.png", false)]
    [TestCase("resources/rgb32rle.tga","resources/rgb32rle.png", true)]
    public void TestGetBitmap(string filename, string expected, bool useAlphaForcefully)
    {
        using FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader r = new BinaryReader(fs);
        
        /* TODO: implement tests actually comparing reference image for each test case.
        var expectedImage = new BitmapImage(new Uri(expected, UriKind.Relative));
        var tga = new TgaImage(r, useAlphaForcefully);
        var actualImage = tga.GetBitmap();

        var expectedConvertedImage = new FormatConvertedBitmap(expectedImage, PixelFormat.Bgra32, null, 0.0);
        var bytesPerPixel = (expectedConvertedImage.Format.BitsPerPixel + 7) / 8;
        var stride = expectedConvertedImage.PixelWidth * bytesPerPixel;
        var expectedImageBytes = new byte[stride * expectedImage.PixelHeight];
        expectedConvertedImage.CopyPixels(expectedImageBytes, stride, 0);

        var actualConvertedImage = new FormatConvertedBitmap(actualImage, PixelFormat.Bgra32, null, 0.0);
        var actualImageBytes = new byte[stride * tga.Header.Height];
        actualConvertedImage.CopyPixels(actualImageBytes, stride, 0);

        CollectionAssert.AreEqual(expectedImageBytes, actualImageBytes, string.Format("expected:{0}, actual:{1}", expected, filename));
        */
    }
}

using NUnit.Framework;

namespace tgalib_core.Tests;

[TestFixture]
[TestOf(typeof(TgaImage))]
public class TgaImageTest
{
    [Test]
    [TestCase("resources/UBW8.TGA", "resources/grayscale_ref.tga", false)]
    [TestCase("resources/UCM8.TGA", "resources/color_ref.tga", false)]
    [TestCase("resources/UTC16.TGA", "resources/color_ref.tga", false)]
    [TestCase("resources/UTC24.TGA", "resources/color_ref.tga", false)]
    [TestCase("resources/UTC32.TGA", "resources/color_ref.tga", false)]
    [TestCase("resources/UBW8.TGA", "resources/grayscale_ref.tga", true)]
    [TestCase("resources/CBW8.TGA", "resources/grayscale_ref.tga", false)]
    [TestCase("resources/CCM8.TGA", "resources/color_ref.tga", false)]
    [TestCase("resources/CTC16.TGA", "resources/color_ref.tga", false)]
    [TestCase("resources/CTC24.TGA", "resources/color_ref.tga", false)]
    [TestCase("resources/CTC32.TGA", "resources/color_ref.tga", false)]
    // [TestCase("resources/rgb32rle.tga","resources/rgb32rle_ref.tga", true)] TODO: review&test processing of Alpha Channel in a future
    public void TestGetImage(string filename, string expectedFilename, bool useAlphaForcefully)
    {
        // Load the actual image
        TgaImage actualImage = new(filename, useAlphaForcefully);

        // Load the expected reference image
        TgaImage expectedImage = new(expectedFilename, useAlphaForcefully);

        // Compare dimensions
        int actualWidth = actualImage.Width;
        int actualHeight = actualImage.Height;
        int expectedWidth = expectedImage.Width;
        int expectedHeight = expectedImage.Height;
        
        Assert.That(actualWidth, Is.EqualTo(expectedWidth), $"Image widths do not match for file: {filename}");
        Assert.That(actualHeight, Is.EqualTo(expectedHeight), $"Image heights do not match for file: {filename}");

        // Compare pixel data
        for (int y = 0; y < actualHeight; y++)
        {
            for (int x = 0; x < actualWidth; x++)
            {
                actualImage.GetPixelRgba(x, y, out int actualR, out int actualG, out int actualB, out int actualA);
                expectedImage.GetPixelRgba(x, y, out int expectedR, out int expectedG, out int expectedB, out int expectedA);

                Assert.That(actualR, Is.EqualTo(expectedR), $"Red component mismatch at ({x}, {y}) in file: {filename}");
                Assert.That(actualG, Is.EqualTo(expectedG), $"Green component mismatch at ({x}, {y}) in file: {filename}");
                Assert.That(actualB, Is.EqualTo(expectedB), $"Blue component mismatch at ({x}, {y}) in file: {filename}");
                if (useAlphaForcefully) { Assert.That(actualA, Is.EqualTo(expectedA), $"Alpha component mismatch at ({x}, {y}) in file: {filename}"); }
            }
        }
    }
}

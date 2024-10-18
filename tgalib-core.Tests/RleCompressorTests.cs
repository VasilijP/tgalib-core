using NUnit.Framework;
using System.IO;

namespace tgalib_core.Tests
{
    [TestFixture]
    public class RleCompressorTests
    {
        [Test]
        public void TestEmptyInput()
        {
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            compressor.ForceWrite(); // Should handle empty input gracefully
            
            Assert.That(0, Is.EqualTo(ms.Length), "Output stream should be empty for empty input.");
        }

        [Test]
        public void TestSinglePixel()
        {
            byte[] pixel = [0xAA];
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            compressor.Write(pixel);
            compressor.ForceWrite();
            
            byte[] expectedOutput1 = [0x00, 0xAA]; // Raw packet with one pixel
            byte[] expectedOutput2 = [ 128, 0xAA]; // Run packet with one pixel
            byte[] actualOutput = ms.ToArray();

            Assert.That(actualOutput, Is.AnyOf(expectedOutput1, expectedOutput2));
        }

        [Test]
        public void TestRunLengthPacket()
        {
            byte[] pixel = [0xAA];
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            // Write the same pixel 5 times
            for (int i = 0; i < 5; i++) { compressor.Write(pixel); }
            compressor.ForceWrite();
            
            // Rep starts at 0 and increments 4 times (total repetitions: 4)
            // Header: 128 | Rep (Rep = 4)
            byte[] expectedOutput = [128 | 4, 0xAA]; // Run-length packet
            byte[] actualOutput = ms.ToArray();

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void TestRunLengthPacket_MaxRun()
        {
            byte[] pixel = [0xAA];
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            // Maximum run-length
            for (int i = 0; i < 128; i++) { compressor.Write(pixel); }
            compressor.ForceWrite();
            
            // Rep increments from 0 to 127 (total repetitions: 127) -> Header: 128 | 127 = 255
            byte[] expectedOutput = [255, 0xAA]; // Run-length packet
            byte[] actualOutput = ms.ToArray();

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void TestRunLengthPacket_ExceedMaxRun()
        {
            byte[] pixel = [0xAA];
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            // Exceed maximum run-length
            for (int i = 0; i < 130; i++) { compressor.Write(pixel); }
            compressor.ForceWrite();
            
            // First packet: Rep = 127 (header: 255), pixel: 0xAA (128 pixels)
            // Second packet: Rep = 1 (header: 130), pixel: 0xAA (2 pixels) -> 130 pixels in total
            byte[] expectedOutput = [255, 0xAA, 128 | 1, 0xAA];
            byte[] actualOutput = ms.ToArray();

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void TestRawPacket()
        {
            byte[] pixel1 = [0xAA];
            byte[] pixel2 = [0xBB];
            byte[] pixel3 = [0xCC];
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            compressor.Write(pixel1);
            compressor.Write(pixel2);
            compressor.Write(pixel3);
            compressor.ForceWrite();
            
            // Raw packet with 3 pixels (header: 2)
            byte[] expectedOutput = [0x02, 0xAA, 0xBB, 0xCC];
            byte[] actualOutput = ms.ToArray();

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void TestMixedPackets()
        {
            byte[] pixelA = [0xAA];
            byte[] pixelB = [0xBB];
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            // Raw pixels
            compressor.Write(pixelA);
            compressor.Write(pixelB);
            compressor.Write(pixelA);

            // Run-length pixels
            for (int i = 0; i < 5; i++) { compressor.Write(pixelB); }
            compressor.ForceWrite();
            
            // Raw packet: header 2, data: 0xAA, 0xBB, 0xAA
            // Run-length packet: header 132 (128 | 4), data: 0xBB
            byte[] expectedOutput = [0x02, 0xAA, 0xBB, 0xAA, 132, 0xBB];
            byte[] actualOutput = ms.ToArray();

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void TestMaxRawPacket()
        {
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            // Raw packet with maximum length (128 pixels)
            for (int i = 0; i < 128; i++) { compressor.Write([(byte)i]); }
            compressor.ForceWrite();
            
            byte[] expectedOutput = new byte[1 + 128];
            expectedOutput[0] = 127; // Header: 128 - 1
            for (int i = 0; i < 128; i++) { expectedOutput[1 + i] = (byte)i; }
            byte[] actualOutput = ms.ToArray();

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void TestRawPacket_ExceedMaxLength()
        {
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            // Write 130 different pixels
            for (int i = 0; i < 130; i++) { compressor.Write([(byte)i]); }
            compressor.ForceWrite();
            
            // First packet: header 127, data: bytes 0-127
            // Second packet: header 1, data: bytes 128-129
            byte[] expectedOutput = new byte[1 + 128 + 1 + 2];
            expectedOutput[0] = 127;
            for (int i = 0; i < 128; i++) { expectedOutput[1 + i] = (byte)i; }
            expectedOutput[129] = 1; // Header for second packet
            expectedOutput[130] = 128;
            expectedOutput[131] = 129;
            byte[] actualOutput = ms.ToArray();

            Assert.That(actualOutput, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void TestInputFillsBuffer()
        {
            const int totalPixels = RleCompressor.Buflen + 1000; // Exceeds buffer length
            using MemoryStream ms = new();
            RleCompressor compressor = new(ms);
            
            for (int i = 0; i < totalPixels; i++) { compressor.Write([(byte)(i % 256)]); }
            // compressor.ForceWrite(); <- This is needed just at the end (when processing an image), the buffer is automatically flushed when it's full during writing.
            
            Assert.That(ms.Length > 0, "Output stream should contain data.");
        }
    }
}

﻿using System.Text;

namespace tgalib_core
{
    /// <summary>
    /// Represents TGA image.
    /// </summary>
    public class TgaImage : IImage
    {
        /// <summary>
        /// Use the alpha channel forcefully, if true.
        /// </summary>
        private readonly bool useAlphaChannelForcefully;

        /// <summary>
        /// Gets or sets a header.
        /// </summary>
        public TgaHeader TgaHeader { get; set; }
        public int Width => TgaHeader.Width;
        public int Height => TgaHeader.Height;
        private int imageWidth;
        
        public PixelFormat PixelFormat { get; set; }

        /// <summary>
        /// Gets or sets an image ID.
        /// </summary>
        public byte[] ImageId { get; set; }

        /// <summary>
        /// Gets or sets a color map(palette).
        /// </summary>
        public byte[] ColorMap { get; set; }

        /// <summary>
        /// Gets or sets an image bytes array.
        /// </summary>
        public byte[] ImageBytes { get; set; }

        /// <summary>
        /// Gets or sets a developer area.
        /// </summary>
        public DeveloperArea DeveloperArea { get; set; }

        /// <summary>
        /// Gets or sets an extension area.
        /// </summary>
        public ExtensionArea ExtensionArea { get; set; }

        /// <summary>
        /// Gets or sets a footer.
        /// </summary>
        public TgaFooter Footer { get; set; }
        
        public TgaImage(string filename, bool useAlphaChannelForcefully = false)
        {
            this.useAlphaChannelForcefully = useAlphaChannelForcefully;
            using FileStream ts = new(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader br = new(ts);
            Init(br);
        }
        
        /// <param name="reader">A binary reader that contains TGA file. Caller must dipose the binary reader.</param>
        /// <param name="useAlphaChannelForcefully">Use the alpha channel forcefully, if true.</param>
        public TgaImage(BinaryReader reader, bool useAlphaChannelForcefully = false)
        {
            this.useAlphaChannelForcefully = useAlphaChannelForcefully;
            Init(reader);
        }
        
        private void Init(BinaryReader reader)
        {
            TgaHeader = new TgaHeader(reader);

            ImageId = new byte[TgaHeader.IdLength];
            reader.Read(ImageId, 0, ImageId.Length);
            PixelFormat = DecodePixelFormat();
            imageWidth = TgaHeader.Width;
            int bytesPerPixel = GetBytesPerPixel();

            ColorMap = new byte[TgaHeader.ColorMapLength * bytesPerPixel];
            reader.Read(ColorMap, 0, ColorMap.Length);

            long position = reader.BaseStream.Position;
            if (TgaFooter.HasFooter(reader))
            {
                Footer = new TgaFooter(reader);

                if (Footer.ExtensionAreaOffset != 0) { ExtensionArea = new ExtensionArea(reader, Footer.ExtensionAreaOffset); }
                if (Footer.DeveloperDirectoryOffset != 0) { DeveloperArea = new DeveloperArea(reader, Footer.DeveloperDirectoryOffset); }
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            ImageBytes = new byte[TgaHeader.Width * TgaHeader.Height * bytesPerPixel];
            ReadImageBytes(reader);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>Returns a string that represents the current object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("[Header]").Append($"{TgaHeader}").AppendLine();
            sb.AppendLine("[Extension Area]");
            sb.Append((Footer != null)?$"{ExtensionArea})":"No extension area").AppendLine();
            sb.AppendLine("[Footer]");
            sb.Append((Footer != null)?$"{Footer}":"No footer");
            return sb.ToString();
        }
        
        // Decoded image data. This allows platform specific conversion to the desired image/texture format.
        // X Origin of Image: Integer ( lo-hi ) X coordinate of the lower left corner of the image.
        // Y Origin of Image: Integer ( lo-hi ) Y coordinate of the lower left corner of the image.
        public void GetPixelRgba(int x, int y, out int r, out int g, out int b, out int a)
        {
            int index = y * imageWidth + x;

            switch (PixelFormat)
            {
                case PixelFormat.Bgra32:
                {
                    int byteIndex = index * 4; // 4 bytes per pixel
                    b = ImageBytes[byteIndex];
                    g = ImageBytes[byteIndex + 1];
                    r = ImageBytes[byteIndex + 2];
                    a = ImageBytes[byteIndex + 3];
                    break;
                }
                case PixelFormat.Bgr24:
                {
                    int byteIndex = index * 3; // 3 bytes per pixel
                    b = ImageBytes[byteIndex];
                    g = ImageBytes[byteIndex + 1];
                    r = ImageBytes[byteIndex + 2];
                    a = 255; // Fully opaque
                    break;
                }
                case PixelFormat.Bgr555:
                {
                    int byteIndex = index * 2; // 2 bytes per pixel
                    ushort value = (ushort)(ImageBytes[byteIndex] | (ImageBytes[byteIndex + 1] << 8));

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
                    int v = ImageBytes[index];
                    r = g = b = v;
                    a = 255; // Fully opaque
                    break;
                }
                default: throw new NotSupportedException($"Pixel format {PixelFormat} is not supported.");
            }
        }

        /// <summary>
        /// Gets a pixel format of TGA image.
        /// </summary>
        /// <returns>Returns a pixel format of TGA image.</returns>
        private PixelFormat DecodePixelFormat()
        {
            switch (TgaHeader.ImageType)
            {
                case TgaMode.ColorMapped:
                case TgaMode.CompressedColorMapped:
                    {
                        // color depth of color-mapped image is defined in the palette
                        switch (TgaHeader.ColorMapDepth)
                        {
                            case ColorDepth.Bpp15:
                            case ColorDepth.Bpp16: return PixelFormat.Bgr555;
                            case ColorDepth.Bpp24: return PixelFormat.Bgr24;
                            case ColorDepth.Bpp32: return PixelFormat.Bgra32;
                            default: throw new NotSupportedException($"Color depth isn't supported({TgaHeader.ColorMapDepth}bpp).");
                        }
                    }

                case TgaMode.TrueColor:
                case TgaMode.CompressedTrueColor:
                    {
                        switch (TgaHeader.PixelDepth)
                        {
                            case ColorDepth.Bpp15:
                            case ColorDepth.Bpp16: return PixelFormat.Bgr555;
                            case ColorDepth.Bpp24: return PixelFormat.Bgr24;
                            case ColorDepth.Bpp32: return PixelFormat.Bgra32;
                            default: throw new NotSupportedException($"Color depth isn't supported({TgaHeader.PixelDepth}bpp).");
                        }
                    }

                case TgaMode.Monochrome:
                case TgaMode.CompressedMonochrome:
                    {
                        switch (TgaHeader.PixelDepth)
                        {
                            case ColorDepth.Bpp8: return PixelFormat.Gray8;
                            default: throw new NotSupportedException($"Color depth isn't supported({TgaHeader.PixelDepth}bpp).");
                        }
                    }

                default: throw new NotSupportedException($"Image type \"{TgaHeader.ImageType}({ImageTypes.ToFormattedText(TgaHeader.ImageType)})\" isn't supported.");
            }
        }

        /// <summary>
        /// Gets bytes per pixel.
        /// </summary>
        /// <returns>Returns bytes per pixel.</returns>
        private int GetBytesPerPixel()
        {
            int bitsPerPixel = 0;
            switch (PixelFormat) // inline table for pixel format at least for Bgra32, Bgr555, Bgr24, Gray8
            {
                case PixelFormat.Bgra32:
                    bitsPerPixel = 32; // 8 bits per channel (Blue, Green, Red, Alpha)
                    break;
                case PixelFormat.Bgr24:
                    bitsPerPixel = 24; // 8 bits per channel (Blue, Green, Red)
                    break;
                case PixelFormat.Bgr555:
                    bitsPerPixel = 16; // 5 bits per channel (Blue, Green, Red), 1 bit unused
                    break;
                case PixelFormat.Gray8:
                    bitsPerPixel = 8; // 8 bits for grayscale
                    break;
                // Add more cases for other pixel formats if needed
                default:
                    throw new NotSupportedException($"Pixel format {PixelFormat} is not supported.");
            }
            return (bitsPerPixel + 7) / 8;
        }

        /// <summary>
        /// Read an image data.
        /// </summary>
        /// <param name="reader">A binary reader that contains TGA file. Caller must dipose the binary reader.</param>
        private void ReadImageBytes(BinaryReader reader)
        {
            switch (TgaHeader.ImageType)
            {
                case TgaMode.ColorMapped:
                case TgaMode.TrueColor:
                case TgaMode.Monochrome:
                    ReadUncompressedData(reader);
                    break;

                case TgaMode.CompressedColorMapped:
                case TgaMode.CompressedTrueColor:
                case TgaMode.CompressedMonochrome:
                    DecodeRunLengthEncoding(reader);
                    break;

                default:
                    throw new NotSupportedException($"Image type \"{TgaHeader.ImageType}({ImageTypes.ToFormattedText(TgaHeader.ImageType)})\" isn't supported.");
            }
        }

        /// <summary>
        /// Reads an uncompressed image data.
        /// </summary>
        /// <param name="reader">A binary reader that contains TGA file. Caller must dipose the binary reader.</param>
        private void ReadUncompressedData(BinaryReader reader)
        {
            // Use a pixel depth, not a color depth. So don't use GetBytesPerPixel().
            // (Pixel data is an index data, if an image type is color-mapped.)
            int bytesPerPixel = (TgaHeader.PixelDepth + 7) / 8;

            int numberOfPixels = TgaHeader.Width * TgaHeader.Height;

            for (int i = 0; i < numberOfPixels; ++i)
            {
                byte[] pixelData = ExtractPixelData(reader.ReadBytes(bytesPerPixel));
                Array.Copy(pixelData, 0, ImageBytes, i * pixelData.Length, pixelData.Length);
            }
        }

        /// <summary>
        /// Decode a run-length encoded data.
        /// </summary>
        /// <param name="reader">A binary reader that contains TGA file. Caller must dipose the binary reader.</param>
        private void DecodeRunLengthEncoding(BinaryReader reader)
        {
            // most significant bit of repetitionCountField deetermins whether run-length packet or raw packet.
            const byte RunLengthPacketMask = 0x80;
            // rest of repetitionCountField represents number of pixels encoded by the packet - 1
            // (actual nmber of pixels encoded by the packet is repetitionCountField + 1)
            const byte RepetitionCountMask = 0x7F;

            // Use a pixel depth, not a color depth. So don't use GetBytesPerPixel().
            // (Pixel data is an index data, if an image type is color-mapped.)
            int bytesPerPixel = (TgaHeader.PixelDepth + 7) / 8;

            int numberOfPixels = TgaHeader.Width * TgaHeader.Height;
            int repetitionCount = 0;
            for (int processedPixels = 0; processedPixels < numberOfPixels; processedPixels += repetitionCount)
            {
                byte repetitionCountField = reader.ReadByte();
                bool isRunLengthPacket = ((repetitionCountField & RunLengthPacketMask) != 0x00);
                repetitionCount = (repetitionCountField & RepetitionCountMask) + 1;

                if (isRunLengthPacket)
                {
                    // Run-length packet
                    byte[] pixelData = ExtractPixelData(reader.ReadBytes(bytesPerPixel));
                    // Repeats same pixel data
                    for (int i = 0; i < repetitionCount; ++i)
                    {
                        Array.Copy(pixelData, 0, ImageBytes, (processedPixels + i) * pixelData.Length, pixelData.Length);
                    }
                }
                else
                {
                    // Raw packet
                    // Repeats different pixel data
                    for (int i = 0; i < repetitionCount; ++i)
                    {
                        byte[] pixelData = ExtractPixelData(reader.ReadBytes(bytesPerPixel));
                        Array.Copy(pixelData, 0, ImageBytes, (processedPixels + i) * pixelData.Length, pixelData.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Extracts a pixel data.
        /// </summary>
        /// <param name="rawPixelData">A raw pixel data in the TGA file.</param>
        /// <returns>
        /// Returns a pixel data in the palette, if an image type is color-mapped.
        /// Returns a raw pixel data, if an image type is RGB or grayscale.
        /// </returns>
        private byte[] ExtractPixelData(byte[] rawPixelData)
        {
            byte[] pixelData = null;

            switch (TgaHeader.ImageType)
            {
                case TgaMode.ColorMapped:
                case TgaMode.CompressedColorMapped:
                    {
                        // Extracts a pixel data in the palette.
                        long paletteIndex = GetPaletteIndex(rawPixelData);
                        int bytesPerPixel = GetBytesPerPixel();
                        byte[] realPixelData = new byte[bytesPerPixel];
                        Array.Copy(ColorMap,
                                   (TgaHeader.ColorMapStart + paletteIndex) * bytesPerPixel,
                                   realPixelData,
                                   0,
                                   realPixelData.Length);
                        pixelData = realPixelData;
                    }
                    break;

                case TgaMode.TrueColor:
                case TgaMode.Monochrome:
                case TgaMode.CompressedTrueColor:
                case TgaMode.CompressedMonochrome:
                    // Returns a raw pixel data as is.
                    pixelData = rawPixelData;
                    break;

                default:
                    throw new NotSupportedException($"Image type \"{TgaHeader.ImageType}({ImageTypes.ToFormattedText(TgaHeader.ImageType)})\" isn't supported.");
            }

            if (!HasAlpha() && !useAlphaChannelForcefully && PixelFormat == PixelFormat.Bgra32)
            {
                pixelData[ArgbOffset.Alpha] = 0xFF;
            }

            return pixelData;
        }

        /// <summary>
        /// Gets a palette index.
        /// </summary>
        /// <param name="indexData">An index data.</param>
        /// <returns>Returns a palette index.</returns>
        private long GetPaletteIndex(byte[] indexData)
        {
            switch (indexData.Length)
            {
                case 1: return indexData[0];
                case 2: return BitConverter.ToUInt16(indexData, 0);
                case 4: return BitConverter.ToUInt32(indexData, 0);
                default: throw new NotSupportedException($"A byte length of index data is not supported({indexData.Length}bytes).");
            }
        }

        /// <summary>
        /// Gets whether has an alpha value or not.
        /// </summary>
        /// <returns>
        /// Returns true, if TGA image has an alpha value.
        /// Returns false, if TGA image don't have an alpha value.
        /// </returns>
        private bool HasAlpha()
        {
            bool hasAlpha = TgaHeader.AttributeBits == 8 || PixelFormat == PixelFormat.Bgra32;

            if (ExtensionArea != null)
            {
                hasAlpha = (ExtensionArea.AttributesType == AttributeTypes.HasAlpha) ||
                           (ExtensionArea.AttributesType == AttributeTypes.HasPreMultipliedAlpha);
            }

            return hasAlpha;
        }
    }
}

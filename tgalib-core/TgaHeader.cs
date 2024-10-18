using System.Text;

namespace tgalib_core;

/// <summary>
/// Represents TGA header.
/// </summary>
public class TgaHeader
{
    /// <summary>
    /// Gets or sets a length of Image ID field.
    /// </summary>
    public byte IdLength { get; set; }

    /// <summary>
    /// Gets or sets a Color Map type(<see cref="ColorMapTypes"/>).
    /// </summary>
    public byte ColorMapType { get; set; }

    /// <summary>
    /// Gets or sets a Image type(<see cref="ImageTypes"/>).
    /// </summary>
    public TgaMode ImageType { get; set; }

    /// <summary>
    /// Gets or sets an offset of first entry in the palette.
    /// </summary>
    public ushort ColorMapStart { get; set; }

    /// <summary>
    /// Gets or sets an entry count in the palette.
    /// </summary>
    public ushort ColorMapLength { get; set; }

    /// <summary>
    /// Gets or sets a number of bits per pixel in the palette entry.
    /// </summary>
    public byte ColorMapDepth { get; set; }

    /// <summary>
    /// Gets or sets X offset.
    /// </summary>
    public ushort XOffset { get; set; }

    /// <summary>
    /// Gets or sets Y offset.
    /// </summary>
    public ushort YOffset { get; set; }

    /// <summary>
    /// Gets or sets a width of image..
    /// </summary>
    public ushort Width { get; set; }

    /// <summary>
    /// Gets or sets a height of image.
    /// </summary>
    public ushort Height { get; set; }

    /// <summary>
    /// Gets or sets a pixel depth.
    /// </summary>
    public byte PixelDepth { get; set; }

    /// <summary>
    /// Gets or sets an image descriptor.
    /// </summary>
    public byte ImageDescriptor { get; set; }

    /// <summary>
    /// Gets a number of bits of attributes per pixel.
    /// </summary>
    public byte AttributeBits { get { return BitsExtractor.Extract(ImageDescriptor, 0, 4); } }

    /// <summary>
    /// Gets an image origin.
    /// </summary>
    public byte ImageOrigin { get { return BitsExtractor.Extract(ImageDescriptor, 4, 2); } }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="reader">A binary reader that contains TGA file. Caller must dipose the binary reader.</param>
    public TgaHeader(BinaryReader reader)
    {
        IdLength = reader.ReadByte();
        ColorMapType = reader.ReadByte();
        ImageType = (TgaMode)reader.ReadByte();
        ColorMapStart = reader.ReadUInt16();
        ColorMapLength = reader.ReadUInt16();
        ColorMapDepth = reader.ReadByte();
        XOffset = reader.ReadUInt16();
        YOffset = reader.ReadUInt16();
        Width = reader.ReadUInt16();
        Height = reader.ReadUInt16();
        PixelDepth = reader.ReadByte();
        ImageDescriptor = reader.ReadByte();
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>Returns a string that represents the current object.</returns>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"IDLength       : {IdLength}\r\n");
        sb.Append($"ColorMapType   : {ColorMapType}({ColorMapTypes.ToFormattedText(ColorMapType)})\r\n");
        sb.Append($"ImageType      : {ImageType}({ImageTypes.ToFormattedText(ImageType)})\r\n");
        sb.Append($"ColorMapStart  : {ColorMapStart}\r\n");
        sb.Append($"ColorMapLength : {ColorMapLength}\r\n");
        sb.Append($"ColorMapDepth  : {ColorMapDepth}\r\n");
        sb.Append($"XOffset        : {XOffset}\r\n");
        sb.Append($"YOffset        : {YOffset}\r\n");
        sb.Append($"Width          : {Width}\r\n");
        sb.Append($"Height         : {Height}\r\n");
        sb.Append($"PixelDepth     : {PixelDepth}\r\n");
        sb.Append($"ImageDescriptor: 0x{ImageDescriptor:X02}(attribute bits: {AttributeBits}, image origin: {ImageOriginTypes.ToFormattedText(ImageOrigin)})\r\n");
        return sb.ToString();
    }
}

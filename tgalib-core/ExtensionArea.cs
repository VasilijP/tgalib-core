using System.Text;

namespace tgalib_core;

/// <summary>
/// Represents TGA extension area.
/// </summary>
public class ExtensionArea
{
    /// <summary>
    /// Field length.
    /// </summary>
    private static class FieldLength
    {
        /// <summary>Author name.</summary>
        public const int AuthorName = 41;

        /// <summary>Author comments.</summary>
        public const int AuthorComments = 324;

        /// <summary>Job name/ID.</summary>
        public const int JobNameID = 41;

        /// <summary>Software ID.</summary>
        public const int SoftwareID = 41;

        /// <summary>Software version letter.</summary>
        public const int SoftwareVersionLetter = 1;
    }
    
    /// <summary>
    /// Gets or sets a size of extension area.
    /// </summary>
    public ushort ExtensionSize { get; set; }

    /// <summary>
    /// Gets or sets an author name.
    /// </summary>
    public string AuthorName { get; set; }

    /// <summary>
    /// Gets or sets author comments.
    /// </summary>
    public string AuthorComments { get; set; }

    /// <summary>
    /// Gets or sets a time-stamp.
    /// </summary>
    public DateTime? TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets a job name/ID.
    /// </summary>
    public string JobNameID { get; set; }

    /// <summary>
    /// Gets or sets a job time.
    /// </summary>
    public TimeSpan JobTime { get; set; }

    /// <summary>
    /// Gets or sets a software ID.
    /// </summary>
    public string SoftwareID { get; set; }

    /// <summary>
    /// Gets or sets a software version.
    /// </summary>
    public string SoftwareVersion { get; set; }

    /// <summary>
    /// Gets or sets a key color.
    /// </summary>
    public uint KeyColor { get; set; }

    /// <summary>
    /// Gets or sets an aspect ratio(width) of pixel.
    /// </summary>
    public ushort PixelAspectRatioWidth { get; set; }

    /// <summary>
    /// Gets or sets an aspect ratio(height) of pixel.
    /// </summary>
    public ushort PixelAspectRatioHeight { get; set; }

    /// <summary>
    /// Gets or sets a gamma value numerator.
    /// </summary>
    public ushort GammaNumerator { get; set; }

    /// <summary>
    /// Gets or sets a gamma value denominator.
    /// </summary>
    public ushort GammaDenominator { get; set; }

    /// <summary>
    /// Gets or sets a color correction offset.
    /// </summary>
    public uint ColorCorrectionOffset { get; set; }

    /// <summary>
    /// Gets or sets a postage stamp offset.
    /// </summary>
    public uint PostageStampOffset { get; set; }

    /// <summary>
    /// Gets or sets a scan line offset.
    /// </summary>
    public uint ScanLineOffset { get; set; }

    /// <summary>
    /// Gets or sets attributes type.
    /// </summary>
    public byte AttributesType { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="reader">
    /// A binary reader that contains TGA file. Caller must dipose the binary reader.
    /// A position of base stream of binary reader roll back in the constructor.
    /// </param>
    /// <param name="extensionAreaOffset">An extension area offset.</param>
    /// <exception cref="InvalidOperationException">
    /// Throws if a base stream of <paramref name="reader"/> doesn't support Seek,
    /// because extension area exists in the position specified an extension area offset in the footer.
    /// </exception>
    public ExtensionArea(BinaryReader reader, uint extensionAreaOffset)
    {
        if (!reader.BaseStream.CanSeek)
        {
            throw new InvalidOperationException("Can't search extension area, because a base stream doesn't support Seek.");
        }

        long originalPosition = reader.BaseStream.Position;
        try
        {
            reader.BaseStream.Seek(extensionAreaOffset, SeekOrigin.Begin);

            ExtensionSize = reader.ReadUInt16();
            AuthorName = reader.ReadString(FieldLength.AuthorName, Encoding.ASCII);
            AuthorComments = reader.ReadString(FieldLength.AuthorComments, Encoding.ASCII);
            TimeStamp = ReadTimeStamp(reader);
            JobNameID = reader.ReadString(FieldLength.JobNameID, Encoding.ASCII);
            JobTime = ReadJobTime(reader);
            SoftwareID = reader.ReadString(FieldLength.SoftwareID, Encoding.ASCII);
            SoftwareVersion = ReadSoftwareVersion(reader);
            KeyColor = reader.ReadUInt32();
            PixelAspectRatioWidth = reader.ReadUInt16();
            PixelAspectRatioHeight = reader.ReadUInt16();
            GammaNumerator = reader.ReadUInt16();
            GammaDenominator = reader.ReadUInt16();
            ColorCorrectionOffset = reader.ReadUInt32();
            PostageStampOffset = reader.ReadUInt32();
            ScanLineOffset = reader.ReadUInt32();
            AttributesType = reader.ReadByte();
        }
        finally
        {
            reader.BaseStream.Position = originalPosition;
        }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>Returns a string that represents the current object.</returns>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"ExtensionSize        : {ExtensionSize}\r\n");
        sb.Append($"AuthorName           : {AuthorName.TrimEnd('\0')}\r\n");
        sb.Append($"AuthorComments       : {AuthorComments.TrimEnd('\0')}\r\n");
        if (TimeStamp.HasValue)
        {
            sb.Append($"TimeStamp            : {TimeStamp.Value:yyyy/MM/dd HH:mm:ss}\r\n");
        }
        else
        {
            sb.AppendLine("TimeStamp            : not specified");
        }

        sb.Append($"Job Name/ID          : {JobNameID.TrimEnd('\0')}\r\n");
        sb.Append($"JobTime              : {JobTime:hh\\:mm\\:ss}\r\n");
        sb.Append($"SoftwareID           : {SoftwareID}\r\n");
        sb.Append($"SoftwareVersion      : {SoftwareVersion}\r\n");
        sb.Append($"KeyColor             : #{KeyColor:X08}\r\n");
        if (PixelAspectRatioHeight != 0)
        {
            sb.Append($"PixelAspectRatio     : {PixelAspectRatioWidth}:{PixelAspectRatioHeight}\r\n");
        }
        else
        {
            sb.AppendLine("PixelAspectRatio     : not specified");
        }
        if (GammaDenominator != 0)
        {
            sb.Append($"GammaValue           : {((double)GammaNumerator) / ((double)GammaDenominator):0.0}\r\n");
        }
        else
        {
            sb.AppendLine("GammaValue           : not specified");
        }

        sb.Append($"ColorCorrectionOffset: {ColorCorrectionOffset}\r\n");
        sb.Append($"PostageStampOffset   : {PostageStampOffset}\r\n");
        sb.Append($"ScanLineOffset       : {ScanLineOffset}\r\n");
        sb.Append($"AttributesType       : {AttributesType}\r\n");
        return sb.ToString();
    }

    /// <summary>
    /// Reads a time-stamp.
    /// </summary>
    /// <param name="reader">A binary reader that contains TGA file. Caller must dipose the binary reader.</param>
    /// <returns>Returns a time-stamp.</returns>
    private DateTime? ReadTimeStamp(BinaryReader reader)
    {
        ushort month = reader.ReadUInt16();
        ushort day = reader.ReadUInt16();
        ushort year = reader.ReadUInt16();
        ushort hour = reader.ReadUInt16();
        ushort minute = reader.ReadUInt16();
        ushort second = reader.ReadUInt16();

        if ((year == 0) && (month == 0) && (day == 0) && (hour == 0) && (minute == 0) && (second == 0))
        {
            return null;
        }
        return new DateTime(year, month, day, hour, minute, second);
    }

    /// <summary>
    /// Read a job time.
    /// </summary>
    /// <param name="reader">A binary reader that contains TGA file. Caller must dipose the binary reader.</param>
    /// <returns>Returns a job time.</returns>
    private TimeSpan ReadJobTime(BinaryReader reader)
    {
        ushort hours = reader.ReadUInt16();
        ushort minutes = reader.ReadUInt16();
        ushort seconds = reader.ReadUInt16();
        return new TimeSpan(hours, minutes, seconds);
    }

    /// <summary>
    /// Reads a software version.
    /// </summary>
    /// <param name="reader">A binary reader that contains TGA file. Caller must dipose the binary reader.</param>
    /// <returns>Returns a software version.</returns>
    private string ReadSoftwareVersion(BinaryReader reader)
    {
        ushort versionNumber = reader.ReadUInt16();
        string versionLetter = reader.ReadString(FieldLength.SoftwareVersionLetter, Encoding.ASCII);
        return $"{((double)versionNumber) / 100.0:0.00}{versionLetter.TrimEnd('\0').TrimEnd(' ')}";
    }
}

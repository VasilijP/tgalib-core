namespace tgalib_core;

/// <summary>
/// Represents TGA developer area.
/// </summary>
public class DeveloperArea
{
    /// <summary>
    /// Gets or sets developer fields.
    /// </summary>
    public DeveloperField[] Fields { get; set; }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="reader">
    /// A binary reader that contains TGA file. Caller must dipose the binary reader.
    /// A position of base stream of binary reader roll back in the constructor.
    /// </param>
    /// <param name="developerAreaOffset">A developer area offset.</param>
    /// <exception cref="InvalidOperationException">
    /// Throws if a base stream of <paramref name="reader"/> doesn't support Seek,
    /// because developer area exists in the position specified a developer area offset in the footer.
    /// </exception>
    public DeveloperArea(BinaryReader reader, uint developerAreaOffset)
    {
        if (!reader.BaseStream.CanSeek)
        {
            throw new InvalidOperationException("Can't search developer area, because a base stream doesn't support Seek.");
        }

        long originalPosition = reader.BaseStream.Position;
        try
        {
            reader.BaseStream.Seek(developerAreaOffset, SeekOrigin.Begin);

            ushort tagCount = reader.ReadUInt16();
            Fields = new DeveloperField[tagCount];
            for (int i = 0; i < tagCount; ++i)
            {
                ushort tag = reader.ReadUInt16();
                uint offset = reader.ReadUInt32();
                uint size = reader.ReadUInt32();
                DeveloperField field = new DeveloperField(reader, tag, offset, size);
                Fields[i] = field;
            }
        }
        finally
        {
            reader.BaseStream.Position = originalPosition;
        }
    }
}

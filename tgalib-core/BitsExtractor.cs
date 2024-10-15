namespace tgalib_core;

/// <summary>
/// Bits extraction utility.
/// </summary>
public static class BitsExtractor
{
    /// <summary>
    /// Extract bits from byte.
    /// </summary>
    /// <param name="value">The 8-bit unsigned value to extract bits.</param>
    /// <param name="bitOffset">A bit offset that starts to extract bits.</param>
    /// <param name="extractBitCount">A bit count to extract.</param>
    /// <returns>Returns extracted bits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The sum of <paramref name="bitOffset"/> and <paramref name="extractBitCount"/> is larger than the 8-bit.
    /// </exception>
    public static byte Extract(byte value, byte bitOffset, byte extractBitCount)
    {
        const byte bitCount = 8;

        if (bitOffset + extractBitCount > bitCount)
        {
            throw new ArgumentOutOfRangeException($"The sum of {nameof(bitOffset)}({bitOffset}) and {nameof(extractBitCount)}({extractBitCount}) is larger than the {bitCount} bit.");
        }

        return (byte)(((uint)value >> bitOffset) & ((uint)byte.MaxValue >> (bitCount - extractBitCount)));
    }
}

using System.Text;

namespace tgalib_core;

/// <summary>
/// Extends <see cref="BinaryReader"/>.
/// </summary>
internal static class BinaryReaderExtension
{
    /// <summary>
    /// Reads a string from <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="reader">The input <see cref="BinaryReader"/>.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <returns>Returns a string to read from <paramref name="reader"/>.</returns>
    public static string ReadString(this BinaryReader reader, int length, Encoding encoding)
    {
        byte[] bytes = reader.ReadBytes(length);
        return encoding.GetString(bytes);
    }
}

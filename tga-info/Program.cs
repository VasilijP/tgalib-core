using tgalib_core;

namespace tga_info
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader reader = new BinaryReader(fs);
            TgaImage tga = new TgaImage(reader);
            Console.WriteLine($"{tga}");
        }
    }
}
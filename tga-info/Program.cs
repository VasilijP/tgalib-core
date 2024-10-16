using tgalib_core;

namespace tga_info
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using FileStream fs = new(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader reader = new(fs);
            TgaImage tga = new(reader);
            Console.WriteLine($"{tga}");
        }
    }
}
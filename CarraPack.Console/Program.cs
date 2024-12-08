using Carra.Carra;

namespace Carra
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //LunarCompress.CompressLunarMod(@"", "");
            Unpacker unpacker = new Unpacker();
            //unpacker.ExtractAssets();
            unpacker.PatchAssets();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs);

        }
    }
}